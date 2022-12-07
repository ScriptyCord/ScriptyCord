using CSharpFunctionalExtensions;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using ScriptCord.Bot;
using ScriptCord.Bot.Dto.Playback;
using ScriptCord.Bot.Services.Playback;
using ScriptCord.Core.DiscordExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptyCord.Bot.Commands.Playback
{
    [Group("playlist", "Manages and plays audio in voice channels")]
    public class PlaylistModule : ScriptyCordCommandModule
    {
        private readonly ILoggerFacade<PlaylistModule> _logger;
        private readonly IPlaylistEntriesService _playlistEntriesService;
        private readonly IPlaylistService _playlistService;

        public PlaylistModule(ILoggerFacade<PlaylistModule> logger, IPlaylistService playlistService, IPlaylistEntriesService playlistEntriesService,
            DiscordSocketClient client, IConfiguration configuration)
        {
            _logger = logger;
            _logger.SetupDiscordLogging(configuration, client, "playback");


            _playlistService = playlistService;
            _playlistEntriesService = playlistEntriesService;
        }

        #region playlistManagement

        [SlashCommand("list-entries", "List the newest entries of a given playlist")]
        public async Task ListEntries([Summary(description: "Name of the playlist")] string playlistName)
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Listing entries in {playlistName} playlist");

            var playlistResult = await _playlistService.GetPlaylistDetails(Context.Guild.Id, playlistName, IsUserGuildAdministrator());
            if (playlistResult.IsFailure)
            {
                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(Discord.Color.Red)
                        .WithTitle("Failure")
                        .WithDescription($"Failed to read playlist's data: {playlistResult.Error}")
                        .Build()
                );
            }

            if (playlistResult.Value.SongCount == 0)
            {
                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(Discord.Color.Blue)
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
                    sb.AppendLine($"**{pair.Index}. *{pair.Entry.Title}*** \n*{pair.Entry.Source} ID: {pair.Entry.SourceId}, Length: {pair.Entry.AudioLength}*");
                    index++;
                }

                if (playlistResult.Value.SongCount >= 15)
                    sb.AppendLine($"\n*and {playlistResult.Value.SongCount-15} more. Use `/playlist export-playlist` to get all entries.*");

                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(Discord.Color.Blue)
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
                     .WithColor(Discord.Color.Red)
                     .WithDescription(playlistsResult.Error)
                     .WithCurrentTimestamp().Build()
                 );
                return;
            }

            IEnumerable<LightPlaylistListingDto> playlists = playlistsResult.Value;
            StringBuilder sb = new StringBuilder();
            int count = 1;
            foreach (var playlist in playlists)
            {
                sb.Append($"**{count}. {playlist.Name}**: {playlist.SongCount} song{(playlist.SongCount > 1 ? "s" : "")}\r\n");
                count++;
            }

            EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle($"{Context.Guild.Name}'s Playlists")
                    .WithColor(Discord.Color.Blue)
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
                        .WithColor(Discord.Color.Green)
                        .WithTitle("Success")
                        .WithDescription($"Created a new playlist called {name}.")
                        .Build()
                );
            }
            else
            {
                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(Discord.Color.Red)
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
                        .WithColor(Discord.Color.Green)
                        .WithTitle("Success")
                        .WithDescription($"Renamed the specified playlist.")
                        .Build()
                );
            }
            else
            {
                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(Discord.Color.Red)
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
            EmbedBuilder embedBuilder = new EmbedBuilder();
            if (result.IsSuccess)
                embedBuilder.WithColor(Discord.Color.Green).WithTitle("Success").WithDescription($"Successfully deleted playlist '{name}'.");
            else
                embedBuilder.WithColor(Discord.Color.Red).WithTitle("Failure").WithDescription($"Failed to remove the specified playlist: {result.Error}");

            await FollowupAsync(embed: embedBuilder.Build());
        }

        [SlashCommand("export-playlist", "Export a given playlist to a json file.")]
        public async Task ExportPlaylist([Summary(description: "Name of the playlist")] string name)
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Exporting a playlist to json");
            var result = await _playlistService.ExportPlaylistToJson(Context.Guild.Id, name, IsUserGuildAdministrator());

            if (result.IsFailure)
            {
                EmbedBuilder embedBuilder = new EmbedBuilder().WithColor(Discord.Color.Red);
                embedBuilder.WithTitle("Failure").WithDescription($"Failed to export the specified playlist: {result.Error}");
                await ReplyAsync(embed: embedBuilder.Build());
            }
            else
            {
                MemoryStream outputStream = new MemoryStream(Encoding.UTF8.GetBytes(result.Value));
                string playlistExportName = $"{name}.json";
                await RespondWithFileAsync(outputStream, playlistExportName);
            }
        }

        #endregion playlistManagement

        #region EntriesManagement

        [SlashCommand("add-entry", "Adds a new entry to the specified playlist")]
        public async Task AddEntry([Summary(description: "Name of the playlist")] string playlistName, [Summary(description: "Link to the video or audio")] string url)
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Adding an entry to a playlist");
            await RespondAsync(embed: CommandIsBeingProcessedEmbed("playback", "add-entry", "Adding entry. This may take a while depending on user traffic and audio length. Please wait..."));

            var result = await _playlistEntriesService.AddEntryFromUrlToPlaylistByName(Context.Guild.Id, playlistName, url);
            EmbedBuilder builder = new EmbedBuilder();

            if (result.IsSuccess)
            {
                var metadata = result.Value;
                string transformedTitle = metadata.Title.Replace("*", "\\*").Replace("|", "\\|").Replace("_", "\\_");
                builder.WithTitle("Success")
                    .WithColor(Discord.Color.Green)
                    .WithDescription($"Successfully added **'{transformedTitle}'** from {metadata.SourceType} to **'{playlistName}'**.\r\n*{url}*")
                    .WithImageUrl(metadata.Thumbnail);
            }
            else
            {
                builder.WithTitle("Failure")
                    .WithColor(Discord.Color.Red)
                    .WithDescription($"Failed to add a new entry to the playlist: {result.Error}!");
            }

            await FollowupAsync(embed: builder.Build());
        }

        [SlashCommand("add-entries-from-playlist", "Adds all entries from a specified playlist url")]
        public async Task AddEntriesFromPlaylist([Summary(description: "Name of the playlist")] string playlistName, [Summary(description: "Url of a playlist")] string url)
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Adding entries to a playlist from a specified playlist url ({url})");

            await RespondAsync(embed: CommandIsBeingProcessedEmbed("playback", "add-entries-from-playlist", "Adding entries from playlist. This may take a while depending on user traffic and amount of videos. Please wait..."));
            
            var message = await FollowupAsync(embed: new EmbedBuilder().WithColor(Discord.Color.Blue).WithDescription($"Download progress: (loading playlist data)").Build());
            Action<int, int, AudioMetadataDto> progressUpdate = (downloadedCount, totalCount, currentMetadata) =>
            {
                if (downloadedCount == totalCount)
                    return;

                string transformedTitle = currentMetadata.Title.Replace("*", "\\*").Replace("|", "\\|").Replace("_", "\\_");
                string description = $"Downloaded: '**{transformedTitle}**'";

                message.ModifyAsync((x) =>
                {
                    x.Embed = new EmbedBuilder()
                    .WithColor(Discord.Color.Blue)
                    .WithTitle($"Download progress: {downloadedCount}/{totalCount}")
                    .WithDescription(description)
                    .WithCurrentTimestamp()
                    .WithImageUrl(currentMetadata.Thumbnail)
                    .Build();
                }).Wait();
            };
            Action<int, int, AudioMetadataDto> finalAction = (finalDownloadedCount, remotePlaylistTotalCount, lastMetadata) =>
            {
                string description = $"All entries from the remote playlist have been successfully added. Added {finalDownloadedCount} out of {remotePlaylistTotalCount} entries (skipped {remotePlaylistTotalCount - finalDownloadedCount} duplicates).\r\n*{url}*";

                message.ModifyAsync((x) =>
                {
                    x.Embed = new EmbedBuilder()
                    .WithColor(Discord.Color.Green)
                    .WithTitle($"Success")
                    .WithDescription(description)
                    .WithCurrentTimestamp()
                    .WithImageUrl(lastMetadata.Thumbnail)
                    .Build();
                }).Wait();
            };

            var result = await _playlistEntriesService.AddEntriesFromPlaylistUrl(Context.Guild.Id, playlistName, url, progressUpdate, finalAction, IsUserGuildAdministrator());
            if (result.IsFailure)
            {
                await message.ModifyAsync((x) =>
                {
                    x.Embed = new EmbedBuilder()
                        .WithColor(Discord.Color.Red)
                        .WithTitle("An error occurred")
                        .WithDescription(result.Error)
                        .Build();
                });
            }
        }

        [SlashCommand("remove-entry", "Removes an entry from the specified playlist")]
        public async Task RemoveEntry([Summary(description: "Name of the playlist")] string playlistName, [Summary(description: "Name of the entry")] string entryName)
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Removing an entry from a playlist");
            await RespondAsync(embed: CommandIsBeingProcessedEmbed("playback", "remove-entry", "Removing the entry. This may take a while depending on user traffic. Please wait..."));

            var result = await _playlistEntriesService.RemoveEntryFromPlaylistByName(Context.Guild.Id, playlistName, entryName);
            EmbedBuilder builder = new EmbedBuilder();

            if (result.IsSuccess)
            {
                var metadata = result.Value;
                builder.WithTitle("Success")
                    .WithColor(Discord.Color.Green)
                    .WithThumbnailUrl(metadata.Thumbnail)
                    .WithDescription($"Successfully removed '{metadata.Title}' from '{playlistName}'.");
            }
            else
            {
                builder.WithTitle("Failure")
                    .WithColor(Discord.Color.Red)
                    .WithDescription($"Failed to remove an entry from the playlist: {result.Error}!");
            }

            await FollowupAsync(embed: builder.Build());
        }

        #endregion EntriesManagement
    }
}
