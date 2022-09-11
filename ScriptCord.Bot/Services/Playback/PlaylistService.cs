using CSharpFunctionalExtensions;
using ScriptCord.Bot.Dto.Playback;
using ScriptCord.Bot.Models.Playback;
using ScriptCord.Bot.Repositories;
using ScriptCord.Bot.Repositories.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Services.Playback
{
    public interface IPlaylistService
    {
        Task<Result<LightPlaylistListingDto>> GetPlaylistDetails(long guildId, string playlistName, bool isAdmin = false);
        string GetEntriesByGuildIdAndPlaylistName(long guildId, string playlistName, bool isAdmin = false);
        Task<Result<IEnumerable<LightPlaylistListingDto>>> GetPlaylistDetailsByGuildIdAsync(long guildId);
        Task<Result> CreateNewPlaylist(long guildId, string playlistName, bool isPremiumUser = false);
        Task<Result> RenamePlaylist(long guildId, string oldPlaylistName, string newPlaylistName, bool isAdmin = false);
        Task<Result> RemovePlaylist(long guildId, string playlistName, bool isAdmin = false);
    }

    public class PlaylistService : IPlaylistService
    {
        private readonly ILoggerFacade<IPlaylistService> _logger;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IPlaylistEntriesRepository _playlistEntriesRepository;

        public PlaylistService(ILoggerFacade<IPlaylistService> logger, IPlaylistRepository playlistRepository, IPlaylistEntriesRepository playlistEntriesRepository)
        {
            _logger = logger;
            _playlistRepository = playlistRepository;
            _playlistEntriesRepository = playlistEntriesRepository;
        }

        public async Task<Result<LightPlaylistListingDto>> GetPlaylistDetails(long guildId, string playlistName, bool isAdmin = false)
        {
            var playlistResult = await _playlistRepository.GetSingleAsync(x => x.GuildId == guildId && x.Name == playlistName);
            if (playlistResult.IsFailure && playlistResult.Value == null)
            {
                _logger.LogError(playlistResult);
                return Result.Failure<LightPlaylistListingDto>("Failed to retrieve the playlist with specified name.");
            }

            var playlist = playlistResult.Value;
            if (playlist.AdminOnly && !isAdmin)
                return Result.Failure<LightPlaylistListingDto>("Only a guild administrator can access information about this playlist!");

            int count = 0;
            IList<LightPlaylistEntryDto> lastFifteenClips = new List<LightPlaylistEntryDto>(); 
            try
            {
                count = playlist.PlaylistEntries.Count();
                lastFifteenClips = playlist.PlaylistEntries.OrderBy(x => x.UploadTimestamp)
                    .Take(15)
                    .Select(x => new LightPlaylistEntryDto { AudioLength = x.AudioLengthFormatted(), Source = x.Source, Id = x.Id.ToString(), SourceId = x.SourceIdentifier, Title = x.Title, UploadedAt = x.UploadTimestamp.ToString("MM/dd/yyyy HH:mm:ss") })
                    .ToList();
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                return Result.Failure<LightPlaylistListingDto>("Unexpected error while trying to get data about entries in the playlist");
            }

            return Result.Success(new LightPlaylistListingDto(playlist.Name, playlist.IsDefault, playlist.AdminOnly, playlist.PlaylistEntries.Count, lastFifteenClips));
        }

        public string GetEntriesByGuildIdAndPlaylistName(long guildId, string playlistName, bool isAdmin = false)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<IEnumerable<LightPlaylistListingDto>>> GetPlaylistDetailsByGuildIdAsync(long guildId)
        {
            var result = await _playlistRepository.GetFiltered(x => x.GuildId == guildId);
            if (result.IsFailure)
            {
                _logger.LogError(result);
                return Result.Failure<IEnumerable<LightPlaylistListingDto>>("Unexpected error occurred while extracting playlist details.");
            }

            var playlists = result.Value;
            return Result.Success(playlists.Select(x => new LightPlaylistListingDto(x.Name, x.IsDefault, x.AdminOnly, x.PlaylistEntries.Count)));
        }

        public async Task<Result> CreateNewPlaylist(long guildId, string playlistName, bool isPremiumUser = false)
        {
            var countResult = await _playlistRepository.CountAsync(x => x.GuildId == guildId && x.Name == playlistName);
            if (countResult.IsSuccess && countResult.Value != 0)
                return Result.Failure("A playlist with the chosen name already exists in this server!");
            else if (countResult.IsFailure)
            {
                _logger.LogError(countResult);
                return Result.Failure("Unexpected error occurred while counting playlists of given name in guild.");
            }

            countResult = await _playlistRepository.CountAsync(x => x.GuildId == guildId);
            if (countResult.IsSuccess && countResult.Value > 0 && !isPremiumUser)
                return Result.Failure("Non-premium users can't create more than one playlist in a server!");
            else if (countResult.IsFailure)
            {
                _logger.LogError(countResult);
                return Result.Failure("Unexpected error occurred while counting playlists in a guild.");
            }

            bool isDefault = true;
            countResult = await _playlistRepository.CountAsync(x => x.GuildId == guildId && x.IsDefault);
            if (countResult.IsSuccess && countResult.Value > 0)
                isDefault = false;
            else if (countResult.IsFailure)
            {
                _logger.LogError(countResult);
                return Result.Failure("Unexpected error occurred while checking if a default playlist already exists in a guild.");
            }

            // TODO: If the user is not premium, he shouldn't be able to create more than one playlist instance

            var model = new Models.Playback.Playlist { Name = playlistName, GuildId = guildId, IsDefault = isDefault, AdminOnly = false };
            var validationResult = model.Validate();
            if (validationResult.IsFailure)
                return validationResult;

            var result = await _playlistRepository.SaveAsync(model);
            if (result.IsFailure)
            {
                _logger.LogError(result);
                return Result.Failure("Unexpected error occurred while creating new playlist.");
            }

            return result;
        }
    
        public async Task<Result> RenamePlaylist(long guildId, string oldPlaylistName, string newPlaylistName, bool isAdmin = false)
        {
            var countResult = await _playlistRepository.CountAsync(x => x.GuildId == guildId && x.Name == newPlaylistName);
            if (countResult.IsSuccess && countResult.Value != 0)
                return Result.Failure("A playlist with the chosen name already exists in this server!");
            else if (countResult.IsFailure)
            {
                _logger.LogError(countResult);
                return Result.Failure("Unexpected error occurred while counting playlists of given name in guild.");
            }
            
            var modelResult = await _playlistRepository.GetSingleAsync(x => x.GuildId == guildId && x.Name == oldPlaylistName);
            if (modelResult.IsFailure)
            {
                _logger.LogError(modelResult);
                return Result.Failure("Unexpected error occurred while creating new playlist.");
            }

            var model = modelResult.Value;
            model.Name = newPlaylistName;

            var validationResult = model.Validate();
            if (validationResult.IsFailure)
                return validationResult;

            if (!isAdmin && model.AdminOnly)
                return Result.Failure("You must be an admin in order to perform this action.");

            var result = await _playlistRepository.UpdateAsync(model);
            if (result.IsFailure)
            {
                _logger.LogError(modelResult);
                return Result.Failure("Unexpected error occurred while updating the playlist entry.");
            }

            return result;
        }

        public async Task<Result> RemovePlaylist(long guildId, string playlistName, bool isAdmin = false)
        {
            //var countResult = await _playlistRepository.CountAsync(x => x.GuildId == guildId && x.Name == playlistName);
            //if (countResult.IsSuccess && countResult.Value == 0)
            //    return Result.Failure("A playlist with the chosen name does not exist in this server!");
            //else if (countResult.IsFailure)
            //{
            //    _logger.LogError(countResult);
            //    return Result.Failure("Unexpected error occurred while searching for specified playlist in guild.");
            //}

            var modelResult = await _playlistRepository.GetSingleAsync(x => x.GuildId == guildId && x.Name == playlistName);
            if (modelResult.IsFailure)
            {
                _logger.LogError(modelResult);
                return Result.Failure("Unexpected error occurred while creating new playlist.");
            }
            else if (modelResult.Value == null)
                return Result.Failure("Specified playlist does not exist in this server");

            var model = modelResult.Value;
            if (model.AdminOnly && !isAdmin)
                return Result.Failure("Only an admin can remove this playlist");

            

            // TODO: Schedule an event for removal of entries' files that are not present in other playlists
            // model.PlaylistEntries.Where()
            model.PlaylistEntries.Clear();
            var deleteManyResult = await _playlistEntriesRepository.DeleteManyAsync(x => x.Playlist.Id == model.Id);
            if (deleteManyResult.IsFailure)
            {
                _logger.LogError(deleteManyResult);
                return Result.Failure("Unexpected error occurred while removing entries of a playlist.");
            }

            var deletePlaylistResult = await _playlistRepository.DeleteAsync(model);
            if (deletePlaylistResult.IsFailure)
            {
                _logger.LogError(deletePlaylistResult);
                return Result.Failure("Unexpected error occurred while removing a playlist.");
            }

            // Switch first different one to a default playlist if not the only playlist
            if (model.IsDefault)
            {
                var otherPlaylistResult = await _playlistRepository.GetFirstAsync(x => x.GuildId == guildId && x.Name != playlistName);
                if (otherPlaylistResult.IsFailure && otherPlaylistResult.Error != "Sequence contains no elements")
                {
                    _logger.LogError(otherPlaylistResult);
                    return Result.Failure("Unexpected error occurred while counting playlists in the server.");
                }
                else if (otherPlaylistResult.Error != "Sequence contains no elements" && otherPlaylistResult.Value != null)
                {
                    var otherModel = otherPlaylistResult.Value;
                    otherModel.IsDefault = true;
                    await _playlistRepository.UpdateAsync(otherModel);
                }
            }

            return Result.Success();
        }
    }
}
