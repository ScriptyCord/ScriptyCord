using CSharpFunctionalExtensions;
using Discord;
using Discord.Interactions;
using ScriptCord.Bot.Dto.Playback;
using ScriptCord.Bot.Repositories;
using ScriptCord.Bot.Repositories.Playback;
using ScriptCord.Bot.Services.Playback;
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
    public class PlaybackModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly Discord.Color _modulesEmbedColor = Discord.Color.DarkRed;
        private readonly LoggerFacade<PlaybackModule> _logger;
        private readonly YoutubeClient _client;

        private IPlaylistService _playlistService;


        public PlaybackModule(LoggerFacade<PlaybackModule> logger, IPlaylistService playlistService)
        {
            _logger = logger;
            _client = new YoutubeClient();
            _playlistService = playlistService;
        }


        //[RequireUserPermission(ChannelPermission.Connect)]
        //[RequireUserPermission(ChannelPermission.Speak)]
        [SlashCommand("list-entries", "Lists entries of a given playlist")]
        // [Choice("playlists", "entries")]
        public async Task ListEntries([Summary(description: "Name of the playlist")] string name)
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Listing entries in {name} playlist");

            // TODO: Perhaps make a BaseScriptCordModule that inherits from interactionmodulebase that has utility methods that returns if a user is an admin etc.
            var guildUser = Context.Guild.Users.FirstOrDefault(x => x.DisplayName == Context.User.Username);
            bool isAdmin;
            if (guildUser == null)
                isAdmin = false;
            else
                isAdmin = guildUser.GuildPermissions.Administrator;

            var playlistResult = await _playlistService.GetPlaylistDetails((long)Context.Guild.Id, name, isAdmin);
            if (playlistResult.IsFailure)
            {
                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(_modulesEmbedColor)
                        .WithTitle("playback list-entries")
                        .WithDescription($"Failed to read playlist's data: {playlistResult.Error}")
                        .Build()
                );
            }

            if (playlistResult.Value.PlaylistEntries.Count == 0)
            {
                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(_modulesEmbedColor)
                        .WithTitle("playback list-entries")
                        .WithDescription($"No entries in this playlist")
                        .Build()
                );
            }
            else if (playlistResult.Value.PlaylistEntries.Count <= 50)
            {
                StringBuilder sb = new StringBuilder();
                int index = 1;
                foreach (var pair in playlistResult.Value.PlaylistEntries.Select(x => new { Index = index, Entry = x }))
                {
                    sb.AppendLine($"**{pair.Index}**. '{pair.Entry.Title}' from {pair.Entry.Source} ({pair.Entry.AudioLengthFormatted()})");
                    index++;
                }
                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(_modulesEmbedColor)
                        .WithTitle("playback list-entries")
                        .WithDescription(sb.ToString())
                        .Build()
                );
            }
            else
            {
                await RespondAsync(
                    // TODO: File export with embed
                    embed: new EmbedBuilder()
                        .WithColor(_modulesEmbedColor)
                        .WithTitle("playback list-entries")
                        .WithDescription("Too many entries in the playlist! In the future a file with entries will be appended.")
                        .Build()
                );
            }
        }

        //[RequireUserPermission(ChannelPermission.Connect)]
        //[RequireUserPermission(ChannelPermission.Speak)]
        [SlashCommand("list-playlists", "Lists playlists")]
        public async Task ListPlaylists()
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Listing server's playlists");
            Result<IEnumerable<PlaylistListingDto>> playlistsResult = await _playlistService.GetPlaylistDetailsByGuildIdAsync((long)Context.Guild.Id);
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

            IEnumerable<PlaylistListingDto> playlists = playlistsResult.Value;
            StringBuilder sb = new StringBuilder();
            int count = 1;
            foreach(var playlist in playlists)
                sb.Append($"**{count}. {playlist.Name}**: songs: {0}, total length: {0}, size: {0}\n");

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
            var isPremiumUser = true;

            var result = await _playlistService.CreateNewPlaylist((long)Context.Guild.Id, name, isPremiumUser);
            if (result.IsSuccess)
            {
                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(_modulesEmbedColor)
                        .WithTitle("playback create-playlist")
                        .WithDescription($"Created a new playlist called {name}.")
                        .Build()
                );
            }
            else
            {
                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(_modulesEmbedColor)
                        .WithTitle("playback create-playlist")
                        .WithDescription($"Failed to create a playlist: {result.Error}")
                        .Build()
                );
            }
        }

        [SlashCommand("rename-playlist", "Renames the selected playlist")]
        public async Task RenamePlaylist([Summary(description: "Old name of the playlist")] string oldName, string newName)
        {
            var guildUser = Context.Guild.Users.FirstOrDefault(x => x.DisplayName == Context.User.Username);
            bool isAdmin;
            if (guildUser == null)
                isAdmin = false;
            else
                isAdmin = guildUser.GuildPermissions.Administrator;

            var result = await _playlistService.RenamePlaylist((long)Context.Guild.Id, oldName, newName, isAdmin);
            if (result.IsSuccess)
            {
                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(_modulesEmbedColor)
                        .WithTitle("playback rename-playlist")
                        .WithDescription($"Renamed the specified playlist.")
                        .Build()
                );
            }
            else
            {
                await RespondAsync(
                    embed: new EmbedBuilder()
                        .WithColor(_modulesEmbedColor)
                        .WithTitle("playback rename-playlist")
                        .WithDescription($"Failed to rename the specified playlist: {result.Error}")
                        .Build()
                );
            }
        }
    }
}
