using CSharpFunctionalExtensions;
using ScriptCord.Bot.Dto.Playback;
using ScriptCord.Bot.Models.Playback;
using ScriptCord.Bot.Repositories;
using ScriptCord.Bot.Repositories.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Services.Playback
{
    public interface IPlaylistService
    {
        int CountEntriesByGuildIdAndPlaylistName(long guildId, string playlistName);
        string GetEntriesByGuildIdAndPlaylistName(long guildId, string playlistName);
        Task<IEnumerable<PlaylistListingDto>> GetPlaylistDetailsByGuildIdAsync(long guildId);
        Task<Result> CreateNewPlaylist(long guildId, string playlistName, bool isPremiumUser = false);
        Task<Result> RenamePlaylist(long guildId, string oldPlaylistName, string newPlaylistName, bool isAdmin = false);
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

        public async Task<Result> CreateNewPlaylist(long guildId, string playlistName, bool isPremiumUser = false)
        {
            if (await _playlistRepository.CountAsync(x => x.GuildId == guildId && x.Name == playlistName) != 0)
                return Result.Failure("A playlist with the chosen name already exists in this server!");

            if (await _playlistRepository.CountAsync(x => x.GuildId == guildId) > 0 && !isPremiumUser)
                return Result.Failure("Non-premium users can't create more than one playlist in a server!");

            bool isDefault = true;
            if (await _playlistRepository.CountAsync(x => x.GuildId == guildId && x.IsDefault) > 0)
                isDefault = false;

            // TODO: If the user is not premium, he shouldn't be able to create more than one playlist instance

            var model = new Models.Playback.Playlist { Name = playlistName, GuildId = guildId, IsDefault = isDefault, AdminOnly = false };
            var validationResult = model.Validate();
            if (validationResult.IsFailure)
                return validationResult;

            var result = await _playlistRepository.InsertAsync(new Models.Playback.Playlist { Name = playlistName, GuildId = guildId, IsDefault = isDefault, AdminOnly = false });
            return result ? Result.Success() : Result.Failure("Unexpected error occurred while adding the playlist.");
        }
    
        public async Task<Result> RenamePlaylist(long guildId, string oldPlaylistName, string newPlaylistName, bool isAdmin = false)
        {
            if (await _playlistRepository.CountAsync(x => x.GuildId == guildId && x.Name == newPlaylistName) != 0)
                return Result.Failure("A playlist with the chosen name already exists in this server!");
            
            
            var model = await _playlistRepository.FindAsync(x => x.GuildId == guildId && x.Name == oldPlaylistName);
            model.Name = newPlaylistName;

            var validationResult = model.Validate();
            if (validationResult.IsFailure)
                return validationResult;

            if (!isAdmin && model.AdminOnly)
                return Result.Failure("You must be an admin in order to perform this action.");

            var result = await _playlistRepository.UpdateAsync(model);
            return result ? Result.Success() : Result.Failure("Unexpected error occurred while renaming the playlist.");
        }
    }
}
