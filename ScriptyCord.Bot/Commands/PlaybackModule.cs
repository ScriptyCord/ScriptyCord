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

namespace ScriptCord.Bot.Commands
{
    [Group("playback", "Manages and plays audio in voice channels")]
    public class PlaybackModule : ScriptCordCommandModule
    {
        private new readonly Discord.Color _modulesEmbedColor = Discord.Color.DarkRed;
        private readonly ILoggerFacade<PlaybackModule> _logger;

        private readonly IPlaylistService _playlistService;
        private readonly IPlaylistEntriesService _playlistEntriesService;
        private readonly PlaybackWorker _playbackWorkerService;

        public PlaybackModule(ILoggerFacade<PlaybackModule> logger, IPlaylistService playlistService, IPlaylistEntriesService playlistEntriesService, PlaybackWorker playbackWorkerService,
            DiscordSocketClient client, IConfiguration configuration)
        {
            _logger = logger;
            _logger.SetupDiscordLogging(configuration, client, "playback");

            _playlistService = playlistService;
            _playlistEntriesService = playlistEntriesService;
            _playbackWorkerService = playbackWorkerService;
        }

        #region PlaylistManagement
        
        [SlashCommand("list-entries", "Lists entries of a given playlist")]
        public async Task ListEntries([Summary(description: "Name of the playlist")] string playlistName)
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Listing entries in {playlistName} playlist");

            var playlistResult = await _playlistService.GetPlaylistDetails(Context.Guild.Id, playlistName, IsUserGuildAdministrator());
            if (playlistResult.IsFailure)
            {
                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(_modulesEmbedColor)
                        .WithTitle("Failure")
                        .WithDescription($"Failed to read playlist's data: {playlistResult.Error}")
                        .Build()
                );
            }

            if (playlistResult.Value.SongCount == 0)
            {
                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(_modulesEmbedColor)
                        .WithTitle($"{playlistName} entries")
                        .WithDescription($"No entries in this playlist")
                        .Build()
                );
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                int index = 1;
                foreach (var pair in playlistResult.Value.NewestFifteenAudioClips.Select(x => new { Index = index, Entry = x }))
                {
                    sb.AppendLine($"**{pair.Index}**. '{pair.Entry.Title}' (ID: {pair.Entry.Id}, {pair.Entry.Source} ID: {pair.Entry.SourceId}, Length: {pair.Entry.AudioLength})");
                    index++;
                }

                if (playlistResult.Value.SongCount >= 15)
                    sb.AppendLine("...");

                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(_modulesEmbedColor)
                        .WithTitle($"{playlistName} entries")
                        .WithDescription(sb.ToString())
                        .Build()
                );
            }
        }

        [SlashCommand("list-playlists", "Lists playlists")]
        public async Task ListPlaylists()
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Listing guild's playlists");
            Result<IEnumerable<LightPlaylistListingDto>> playlistsResult = await _playlistService.GetPlaylistDetailsByGuildIdAsync(Context.Guild.Id);
            if (playlistsResult.IsFailure)
            {
                await RespondAsync(embed: new EmbedBuilder()
                     .WithTitle($"{Context.Guild.Name}'s Playlists")
                     .WithColor(_modulesEmbedColor)
                     .WithDescription(playlistsResult.Error)
                     .WithCurrentTimestamp().Build()
                 );
                return;
            }

            IEnumerable<LightPlaylistListingDto> playlists = playlistsResult.Value;
            StringBuilder sb = new StringBuilder();
            int count = 1;
            foreach(var playlist in playlists)
            {
                sb.Append($"**{count}. {playlist.Name}**: {playlist.SongCount} song{(playlist.SongCount > 1 ? "s" : "")}\r\n");
                count++;
            }

            EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle($"{Context.Guild.Name}'s Playlists")
                    .WithColor(_modulesEmbedColor)
                    .WithCurrentTimestamp();
            if (playlists.Count() > 0)
                builder.WithDescription(sb.ToString());
            else
                builder.WithDescription("no playlists are registered in this server");

            await RespondAsync(embed: builder.Build());
        }

        [SlashCommand("create-playlist", "Creates a playlist with the given name")]
        public async Task CreatePlaylist([Summary(description: "Name of the playlist")] string name)
        {
            // TODO: check if user is from "premium" users that can create multiple playlists
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Creating a playlist");
            var isPremiumUser = true;

            var result = await _playlistService.CreateNewPlaylist(Context.Guild.Id, name, isPremiumUser);
            if (result.IsSuccess)
            {
                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(_modulesEmbedColor)
                        .WithTitle("Success")
                        .WithDescription($"Created a new playlist called {name}.")
                        .Build()
                );
            }
            else
            {
                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(_modulesEmbedColor)
                        .WithTitle("Failure")
                        .WithDescription($"Failed to create a playlist: {result.Error}")
                        .Build()
                );
            }
        }

        [SlashCommand("rename-playlist", "Renames the selected playlist")]
        public async Task RenamePlaylist([Summary(description: "Old name of the playlist")] string oldName, string newName)
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Renaming a playlist");
            var result = await _playlistService.RenamePlaylist(Context.Guild.Id, oldName, newName, IsUserGuildAdministrator());
            if (result.IsSuccess)
            {
                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(_modulesEmbedColor)
                        .WithTitle("Success")
                        .WithDescription($"Renamed the specified playlist.")
                        .Build()
                );
            }
            else
            {
                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(_modulesEmbedColor)
                        .WithTitle("Failure")
                        .WithDescription($"Failed to rename the specified playlist: {result.Error}")
                        .Build()
                );
            }
        }

        [SlashCommand("remove-playlist", "Removes the selected playlist")]
        public async Task RemovePlaylist([Summary(description: "Name of the playlist")] string name)
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Removing a playlist");
            await RespondAsync(embed: CommandIsBeingProcessedEmbed("playback", "remove-playlist", "Removing a playlist and its entries. This may take a while depending on user traffic and amount of entries. Please wait..."));

            var result = await _playlistService.RemovePlaylist(Context.Guild.Id, name, IsUserGuildAdministrator());
            EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(_modulesEmbedColor);
            if (result.IsSuccess)
                embedBuilder.WithTitle("Success").WithDescription($"Successfully deleted playlist '{name}'.");
            else
                embedBuilder.WithTitle("Failure").WithDescription($"Failed to remove the specified playlist: {result.Error}");

            await FollowupAsync(embed: embedBuilder.Build());
        }

        #endregion PlaylistManagement

        #region EntriesManagement

        [SlashCommand("add-entry", "Adds a new entry to the specified playlist")]
        public async Task AddEntry([Summary(description: "Name of the playlist")] string playlistName, [Summary(description: "Link to the video or audio")] string url)
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Adding an entry to a playlist");
            await RespondAsync(embed: CommandIsBeingProcessedEmbed("playback", "add-entry", "Adding entry. This may take a while depending on user traffic and audio length. Please wait..."));

            var result = await _playlistEntriesService.AddEntryFromUrlToPlaylistByName(Context.Guild.Id, playlistName, url);
            EmbedBuilder builder = new EmbedBuilder().WithColor(_modulesEmbedColor);

            if (result.IsSuccess)
            {
                var metadata = result.Value;
                builder.WithTitle("Success")
                    .WithThumbnailUrl(metadata.Thumbnail)
                    .WithDescription($"Successfully added '{metadata.Title}' from {metadata.SourceType} to '{playlistName}'.");
            }
            else
            {
                builder.WithTitle("Failure")
                    .WithDescription($"Failed to add a new entry to the playlist: {result.Error}!");
            }

            await FollowupAsync(embed: builder.Build());
        }

        [SlashCommand("add-entries-from-playlist", "Adds all entries from a specified playlist url")]
        public async Task AddEntriesFromPlaylist([Summary(description: "Name of the playlist")] string playlistName, [Summary(description: "Url of a playlist")] string url)
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Adding entries to a playlist from a specified playlist url ({url})");

            await RespondAsync(embed: new EmbedBuilder().WithColor(_modulesEmbedColor).WithDescription($"Adding entries from internet playlist: {url}").Build());
            var message = await ReplyAsync(embed: new EmbedBuilder().WithColor(_modulesEmbedColor).WithDescription($"Download progress: (loading playlist data)").Build());
            var result = await _playlistEntriesService.AddEntriesFromPlaylistUrl(Context.Guild.Id, playlistName, url, async (downloadedCount, totalCount, currentMetadata) =>
            {
                string description = $"Downloaded: ' **{currentMetadata.Title} **'";
                if (downloadedCount == totalCount)
                    description += ".\r\n*All entries have been successfully added.*";

                await message.ModifyAsync((x) => 
                { 
                    x.Embed = new EmbedBuilder()
                    .WithColor(_modulesEmbedColor)
                    .WithTitle($"Download progress: {downloadedCount}/{totalCount}")
                    .WithDescription(description)
                    .WithCurrentTimestamp()
                    .WithImageUrl(currentMetadata.Thumbnail)
                    .Build();
                });
            }, IsUserGuildAdministrator());
            if (result.IsFailure)
            {
                await FollowupAsync(embed: new EmbedBuilder().WithTitle("Failure").WithDescription($"Failed to add entries from {url}").Build());
            }
        }

        [SlashCommand("remove-entry", "Removes an entry from the specified playlist")]
        public async Task RemoveEntry([Summary(description: "Name of the playlist")] string playlistName, [Summary(description: "Name of the entry")] string entryName)
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Removing an entry from a playlist");
            await RespondAsync(embed: CommandIsBeingProcessedEmbed("playback", "remove-entry", "Removing the entry. This may take a while depending on user traffic. Please wait..."));

            var result = await _playlistEntriesService.RemoveEntryFromPlaylistByName(Context.Guild.Id, playlistName, entryName);
            EmbedBuilder builder = new EmbedBuilder().WithColor(_modulesEmbedColor);

            if (result.IsSuccess)
            {
                var metadata = result.Value;
                builder.WithTitle("Success")
                    .WithThumbnailUrl(metadata.Thumbnail)
                    .WithDescription($"Successfully removed '{metadata.Title}' from '{playlistName}'.");
            }
            else
            {
                builder.WithTitle("Failure")
                    .WithDescription($"Failed to remove an entry from the playlist: {result.Error}!");
            }

            await FollowupAsync(embed: builder.Build());
        }

        #endregion EntriesManagement

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
