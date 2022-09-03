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
using YoutubeExplode;

namespace ScriptCord.Bot.Commands
{
    [Group("playback", "Manages and plays audio in voice channels")]
    public class PlaybackModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly LoggerFacade<PlaybackModule> _logger;
        private readonly YoutubeClient _client;

        private IPlaylistService _playlistService;

        public PlaybackModule(LoggerFacade<PlaybackModule> logger, IPlaylistService playlistService)
        {
            _logger = logger;
            _client = new YoutubeClient();
            _playlistService = playlistService;
        }


        #region playlist

        [RequireUserPermission(ChannelPermission.Connect)]
        [RequireUserPermission(ChannelPermission.Speak)]
        [SlashCommand("list-entries", "Lists entries of a given playlist")]
        // [Choice("playlists", "entries")]
        public async Task List([Summary(description: "Name of the playlist")] string name)
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Listing entries in {name} playlist"); 
            var count = _playlistService.CountEntriesByGuildIdAndPlaylistName((long) Context.Guild.Id, name);
            EmbedBuilder builder = new EmbedBuilder().WithCurrentTimestamp();

            if (count == 0)
                builder.WithDescription($"No playlist named '{name}' was found in this server");
            else if (count < 50)
            {
                await RespondAsync(_playlistService.GetEntriesByGuildIdAndPlaylistName((long)Context.Guild.Id, name));
                builder.WithDescription($"Feature not fully implemented yet");
            }
            else
            {
                builder.WithDescription($"Feature not fully implemented yet");
            }

            await RespondAsync(embed: builder.Build());
        }

        //[RequireUserPermission(ChannelPermission.Connect)]
        //[RequireUserPermission(ChannelPermission.Speak)]
        [SlashCommand("list-playlists", "Lists playlists")]
        public async Task List()
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Listing server's playlists");
            IEnumerable<PlaylistListingDto> playlists = await _playlistService.GetPlaylistDetailsByGuildIdAsync((long)Context.Guild.Id);
            StringBuilder sb = new StringBuilder();
            int count = 1;
            foreach(var playlist in playlists)
                sb.Append($"**{count}.{playlist.Name}**: songs: {0}, total length: {0}, size: {0}\n");

            EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle($"{Context.Guild.Name}'s Playlists")
                    .WithColor(Color.DarkRed)
                    .WithCurrentTimestamp();
            if (playlists.Count() > 0)
                builder.WithDescription(sb.ToString());
            else
                builder.WithDescription("no playlists are registered in this server");

            await RespondAsync(embed: builder.Build());
        }

        #endregion playlist
    }
}
