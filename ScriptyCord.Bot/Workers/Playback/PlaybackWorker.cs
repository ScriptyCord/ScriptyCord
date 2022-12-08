using Discord.Audio;
using Discord;
using FluentNHibernate.Conventions;
using ScriptCord.Bot.Dto.Playback;
using ScriptCord.Bot.Events;
using ScriptCord.Bot.Events.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using ScriptyCord.Bot.Events.Playback;

namespace ScriptCord.Bot.Workers.Playback
{
    public interface IPlaybackWorker : IWorker
    {
        bool HasPlaybackSession(ulong guildId);
        PlaylistEntryDto GetPlaybackSessionData(ulong guildId);
        TimeSpan GetTimeSinceEntryStart(ulong guildId);
        public int GetPlaybackSessionsCount();
    }

    public class PlaybackWorker : IPlaybackWorker
    {
        private readonly ILoggerFacade<IPlaybackWorker> _logger;

        private Thread _thread;

        public static Queue<IExecutableEvent> Events { get; } = new Queue<IExecutableEvent>();

        public static Queue<(NLog.LogLevel, string)> EventLogsQueue { get; } = new Queue<(NLog.LogLevel, string)>();

        private Dictionary<ulong, IPlaybackSession> _sessions;

        private bool _stop = false;

        public PlaybackWorker(ILoggerFacade<IPlaybackWorker> logger, IConfiguration configuration, DiscordSocketClient client)
        {
            _logger = logger;
            _logger.SetupDiscordLogging(configuration, client, "playback");

            _sessions = new Dictionary<ulong, IPlaybackSession>();
        }

        public async Task Run()
        {
            _thread = new Thread(async () => { await run(); });
            _thread.Start();
        }

        public async Task run()
        {
            _logger.LogInfo("Starting worker execution");
            while (!_stop)
            {
                int executed = 0;
                while (Events.IsNotEmpty())
                {
                    var playbackEvent = Events.Dequeue();
                    executed++;

                    if (playbackEvent is PlaySongEvent && !_sessions.ContainsKey(playbackEvent.GuildId))
                    {
                        var castedEvent = (PlaySongEvent)playbackEvent;
                        _sessions[playbackEvent.GuildId] = new PlaybackSession(castedEvent.Playlist, castedEvent.Client, castedEvent.GuildId);
                        _sessions[playbackEvent.GuildId].StartPlaybackThread();
                    }
                    else if (_sessions.ContainsKey(playbackEvent.GuildId))
                    {
                        if (playbackEvent is SkipSongEvent)
                            _sessions[playbackEvent.GuildId].SkipSong();
                        else if (playbackEvent is PauseSongEvent)
                            _sessions[playbackEvent.GuildId].PausePlayback();
                        else if (playbackEvent is UnpauseSongEvent)
                            _sessions[playbackEvent.GuildId].UnpausePlayback();
                        else if (playbackEvent is StopPlaybackEvent)
                        {
                            _sessions[playbackEvent.GuildId].StopPlaybackThread();
                            _sessions.Remove(playbackEvent.GuildId);
                        }
                        else if (playbackEvent is AppendSongsEvent)
                            _sessions[playbackEvent.GuildId].AppendSongs(((AppendSongsEvent)playbackEvent).NewEntries);
                    }
                }

                while (EventLogsQueue.IsNotEmpty())
                {
                    var log = EventLogsQueue.Dequeue();
                    _logger.Log(log.Item1, log.Item2);
                }

                if (executed > 0)
                    _logger.LogInfo($"Executed {executed} playback events");

                await Task.Delay(500);
            }
        }

        public void Stop()
            => _stop = true;

        public bool HasPlaybackSession(ulong guildId)
            => _sessions.ContainsKey(guildId);

        public PlaylistEntryDto GetPlaybackSessionData(ulong guildId) => _sessions[guildId].GetCurrentlyPlayingEntry();

        public TimeSpan GetTimeSinceEntryStart(ulong guildId) => _sessions[guildId].GetTimeSinceEntryStart();

        public int GetPlaybackSessionsCount()
            => _sessions.Count;
    }

    public interface IPlaybackSession
    {
        void StartPlaybackThread();
        void SkipSong();
        void PausePlayback();
        void UnpausePlayback();
        void StopPlaybackThread();
        void AppendSongs(IList<PlaylistEntryDto> newEntries);
        PlaylistEntryDto GetCurrentlyPlayingEntry();
        TimeSpan GetTimeSinceEntryStart();
    }

    public class PlaybackSession : IPlaybackSession
    {
        private IList<PlaylistEntryDto> _playlist;

        private Thread _playbackThread;

        private IAudioClient _client;

        private CancellationTokenSource _cancellationTokenSource;

        private bool _stopPlayback = false;

        private bool _pausePlayback = false;

        private ulong _guildId;

        private DateTime _startedCurrentEntryAt;

        public PlaybackSession(IList<PlaylistEntryDto> playlist, IAudioClient client, ulong guildId)
        {
            _playlist = playlist;
            _client = client;
            _guildId = guildId;
        }

        public void StartPlaybackThread()
        {
            _playbackThread = new Thread(new ThreadStart(
                () =>
                {
                    var resultTask = PlayInBackground();
                    resultTask.Wait();
                }
            ));
            _playbackThread.Start();
        }

        public void SkipSong()
        {
            _cancellationTokenSource.Cancel();
        }

        public void PausePlayback()
        {
            _pausePlayback = true;
            _cancellationTokenSource.Cancel();
        }

        public void UnpausePlayback() => _pausePlayback = false;

        public void StopPlaybackThread()
        {
            _stopPlayback = true;
            _cancellationTokenSource.Cancel();
        }

        public void AppendSongs(IList<PlaylistEntryDto> newEntries)
        {
            _playlistEditSemaphore.WaitOne();

            foreach (var entry in newEntries)
                //_playlist.Append(entry);
                _playlist.Add(entry);

            _playlistEditSemaphore.Release(releaseCount: 1);
        }

        public PlaylistEntryDto GetCurrentlyPlayingEntry() => _playlist[0];

        public TimeSpan GetTimeSinceEntryStart() => DateTime.Now - _startedCurrentEntryAt;

        private Semaphore _playlistEditSemaphore = new Semaphore(1, 1);

        private async Task PlayInBackground()
        {
            while (_playlist.Count > 0)
            {
                _playlistEditSemaphore.WaitOne();
                if (_stopPlayback)
                    break;

                if (_pausePlayback)
                {
                    while (_pausePlayback)
                        Thread.Sleep(1000);
                }
                _playlistEditSemaphore.Release(releaseCount: 1);

                var currentEntry = _playlist[0];

                _cancellationTokenSource = new CancellationTokenSource();

                using (var ffmpeg = CreateStream(currentEntry.Path))
                using (var stream = _client.CreatePCMStream(AudioApplication.Music))
                {
                    try
                    {
                        _startedCurrentEntryAt = DateTime.Now;
                        await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream, _cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException e) 
                    {
                        if (e.Message == "The operation was canceled.")
                            PlaybackWorker.EventLogsQueue.Enqueue((NLog.LogLevel.Info, $"Possibly web socket is reconnecting. {e.Data}"));
                        PlaybackWorker.EventLogsQueue.Enqueue((NLog.LogLevel.Info, e.Message));
                    }
                    catch (Exception e)
                    {
                        PlaybackWorker.EventLogsQueue.Enqueue((NLog.LogLevel.Error, e.Message));
                    }
                    finally { await stream.FlushAsync(); }
                }

                _playlistEditSemaphore.WaitOne();
                if (!_pausePlayback)
                    _playlist.RemoveAt(0);
                _playlistEditSemaphore.Release(releaseCount: 1);
            }
            PlaybackWorker.Events.Enqueue(new StopPlaybackEvent(_guildId));
            await _client.StopAsync();
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }
    }
}
