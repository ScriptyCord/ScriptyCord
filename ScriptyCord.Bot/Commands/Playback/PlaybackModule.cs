using CSharpFunctionalExtensions;
using Discord;
using Discord.Audio;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using ScriptCord.Bot.Dto.Playback;
using ScriptCord.Bot.Events.Playback;
using ScriptCord.Bot.Repositories;
using ScriptCord.Bot.Repositories.Playback;
using ScriptCord.Bot.Services.Playback;
using ScriptCord.Bot.Workers.Playback;
using ScriptCord.Core.DiscordExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YoutubeExplode;

namespace ScriptCord.Bot.Commands.Playback
{
    [Group("playback", "Manages and plays audio in voice channels")]
    public class PlaybackModule : ScriptCordCommandModule
    {
        private new readonly Discord.Color _modulesEmbedColor = Discord.Color.DarkRed;
        private readonly ILoggerFacade<PlaybackModule> _logger;

        private readonly IPlaylistService _playlistService;
        private readonly IPlaylistEntriesService _playlistEntriesService;
        private readonly IPlaybackWorker _playbackWorkerService;

        public PlaybackModule(ILoggerFacade<PlaybackModule> logger, IPlaylistService playlistService, IPlaylistEntriesService playlistEntriesService, IPlaybackWorker playbackWorkerService,
            DiscordSocketClient client, IConfiguration configuration)
        {
            _logger = logger;
            _logger.SetupDiscordLogging(configuration, client, "playback");

            _playlistService = playlistService;
            _playlistEntriesService = playlistEntriesService;
            _playbackWorkerService = playbackWorkerService;
        }

        #region PlaybackManagement

        [SlashCommand("play", "Plays the selected playlist in the voice chat that the user is currently in")]
        public async Task Play([Summary(description: "Name of the playlist")] string playlistName)
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Starting playlback of the specified playlist");
            if (_playbackWorkerService.HasPlaybackSession(Context.Guild.Id))
            {
                await RespondAsync(embed: new EmbedBuilder().WithColor(_modulesEmbedColor).WithTitle("Failure").WithDescription("Bot is already playing in your server!").Build());
                return;
            }

            IVoiceChannel channel = null;
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            
            EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(_modulesEmbedColor);
            if (channel is null)
                embedBuilder.WithTitle("Failure").WithDescription("You are not in a voice channel!");
            else
                embedBuilder.WithDescription("Joining your voice channel...");

            // TODO: First check if already connected to the current voice channel or another one
            await RespondAsync(embed: embedBuilder.Build());

            if (channel is not null)
            {
                var shuffledEntriesResult = await _playlistService.GetShuffledEntries(Context.Guild.Id, playlistName, IsUserGuildAdministrator());
                if (shuffledEntriesResult.IsFailure)
                {
                    await FollowupAsync(embed: new EmbedBuilder().WithColor(_modulesEmbedColor).WithTitle("Playback failure").WithDescription(shuffledEntriesResult.Error).Build());
                    return;
                }

                IAudioClient client = await channel.ConnectAsync();
                PlaySongEvent playbackEvent = new PlaySongEvent(client, channel, Context.Guild.Id, shuffledEntriesResult.Value);
                PlaybackWorker.Events.Enqueue(playbackEvent);
            }
        }

        [SlashCommand("stop", "Stops playback and leaves the voice chat")]
        public async Task Stop()
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Stopping playlback in voice chat");
            if (!_playbackWorkerService.HasPlaybackSession(Context.Guild.Id))
            {
                await RespondAsync(embed: new EmbedBuilder().WithColor(_modulesEmbedColor).WithTitle("Failure").WithDescription("Bot is not playing in your server!").Build());
                return;
            }

            IVoiceChannel channel = null;
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;

            EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(_modulesEmbedColor);
            if (channel is null)
                embedBuilder.WithTitle("Failure").WithDescription("You are not in a voice channel!");
            else
                embedBuilder.WithDescription("Stopping and leaving your voice channel...");

            await RespondAsync(embed: embedBuilder.Build());
            if (channel is not null)
            {
                StopPlaybackEvent stopPlaybackEvent = new StopPlaybackEvent(Context.Guild.Id);
                PlaybackWorker.Events.Enqueue(stopPlaybackEvent);
            }
        }

        [SlashCommand("pause", "Pauses playback of the current song without leaving the voice channel")]
        public async Task Pause()
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Pausing playlback in voice chat");
            if (!_playbackWorkerService.HasPlaybackSession(Context.Guild.Id))
            {
                await RespondAsync(embed: new EmbedBuilder().WithColor(_modulesEmbedColor).WithTitle("Failure").WithDescription("Bot is not playing in your server!").Build());
                return;
            }

            IVoiceChannel channel = null;
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;

            EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(_modulesEmbedColor);
            if (channel is null)
                embedBuilder.WithTitle("Failure").WithDescription("You are not in a voice channel!");
            else
                embedBuilder.WithDescription("Pausing playback...");

            await RespondAsync(embed: embedBuilder.Build());
            if (channel is not null)
            {
                PauseSongEvent pauseSongEvent = new PauseSongEvent(Context.Guild.Id);
                PlaybackWorker.Events.Enqueue(pauseSongEvent);
            }
        }

        [SlashCommand("unpause", "Unpauses playback")]
        public async Task Unpause()
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Unpausing playlback in voice chat");
            if (!_playbackWorkerService.HasPlaybackSession(Context.Guild.Id))
            {
                await RespondAsync(embed: new EmbedBuilder().WithColor(_modulesEmbedColor).WithTitle("Failure").WithDescription("Bot is not playing in your server!").Build());
                return;
            }

            IVoiceChannel channel = null;
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;

            EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(_modulesEmbedColor);
            if (channel is null)
                embedBuilder.WithTitle("Failure").WithDescription("You are not in a voice channel!");
            else
                embedBuilder.WithDescription("Unpausing playback...");

            await RespondAsync(embed: embedBuilder.Build());
            if (channel is not null)
            {
                UnpauseSongEvent unpauseSongEvent = new UnpauseSongEvent(Context.Guild.Id);
                PlaybackWorker.Events.Enqueue(unpauseSongEvent);
            }
        }

        [SlashCommand("skip", "Skips the current song and starts playing next song")]
        public async Task Next()
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Skipping to next song in voice chat");
            if (!_playbackWorkerService.HasPlaybackSession(Context.Guild.Id))
            {
                await RespondAsync(embed: new EmbedBuilder().WithColor(_modulesEmbedColor).WithTitle("Failure").WithDescription("Bot is not playing in your server!").Build());
                return;
            }

            IVoiceChannel channel = null;
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;

            EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(_modulesEmbedColor);
            if (channel is null)
                embedBuilder.WithTitle("Failure").WithDescription("You are not in a voice channel!");
            else
                embedBuilder.WithDescription("Skipping to next song...");

            await RespondAsync(embed: embedBuilder.Build());
            if (channel is not null)
            {
                SkipSongEvent skipSongEvent = new SkipSongEvent(Context.Guild.Id);
                PlaybackWorker.Events.Enqueue(skipSongEvent);
            }
        }

        [SlashCommand("now-playing", "Get information about the currently playing entry")]
        public async Task NowPlaying()
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Checking information about currently playing song in voice chat");
            
            EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(_modulesEmbedColor);
            var dataResult = await _playlistService.GetCurrentlyPlayingMetadata(Context.Guild.Id);
            if (dataResult.IsFailure)
                embedBuilder.WithTitle("Failure").WithDescription($"{dataResult.Error}!");
            else
            {
                var data = dataResult.Value;
                TimeSpan timeSinceStart = _playbackWorkerService.GetTimeSinceEntryStart(Context.Guild.Id);
                TimeSpan totalTime = TimeSpan.FromMilliseconds(data.AudioLength);

                string intervalCurrentString = timeSinceStart.ToString(@"mm\:ss");
                string intervalTotalString = totalTime.ToString(@"mm\:ss");

                embedBuilder.WithTitle("Currently playing")
                    .WithDescription($"**{data.Title}** from {data.SourceType} ({intervalCurrentString}/{intervalTotalString})\r\n{data.Url}").WithImageUrl(data.Thumbnail);
            }

            await RespondAsync(embed: embedBuilder.Build());
        }

        #endregion
    }
}
