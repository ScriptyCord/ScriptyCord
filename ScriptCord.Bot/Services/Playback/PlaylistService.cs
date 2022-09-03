using ScriptCord.Bot.Dto.Playback;
using ScriptCord.Bot.Repositories;
using ScriptCord.Bot.Repositories.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Services.Playback
{
    public interface IPlaylistService
    {
        int CountEntriesByGuildIdAndPlaylistName(long guildId, string playlistName);
        string GetEntriesByGuildIdAndPlaylistName(long guildId, string playlistName);
        Task<IEnumerable<PlaylistListingDto>> GetPlaylistDetailsByGuildIdAsync(long guildId);
        Task<bool> CreateNewPlaylist(long guildId, string playlistName, bool isDefault);
    }

    public class PlaylistService : IPlaylistService
    {
        private readonly LoggerFacade<IPlaylistService> _logger;
        private readonly IPlaylistRepository _playlistRepository;

        public PlaylistService(LoggerFacade<IPlaylistService> logger, IScriptCordUnitOfWork uow)
        {
            _playlistRepository = uow.PlaylistRepository;
            _logger = logger;
        }

        public int CountEntriesByGuildIdAndPlaylistName(long guildId, string playlistName)
        {
            return 0;
        }

        public string GetEntriesByGuildIdAndPlaylistName(long guildId, string playlistName)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<PlaylistListingDto>> GetPlaylistDetailsByGuildIdAsync(long guildId)
        {
            var playlists = await _playlistRepository.FindAllAsync(x => x.GuildId == guildId);
            return playlists.Select(x => new PlaylistListingDto(x.Name, x.IsDefault, x.AdminOnly));
        }

        public async Task<bool> CreateNewPlaylist(long guildId, string playlistName, bool isDefault)
            => await _playlistRepository.InsertAsync(new Models.Playback.Playlist { Name = playlistName, GuildId = guildId, IsDefault = isDefault, AdminOnly = false });
    }
}
