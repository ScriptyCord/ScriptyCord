using CSharpFunctionalExtensions;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using ScriptCord.Bot.Dto.Playback;
using ScriptCord.Bot.Models.Playback;
using ScriptCord.Bot.Repositories;
using ScriptCord.Bot.Repositories.Playback;
using ScriptCord.Bot.Strategies.AudioManagement;
using ScriptCord.Bot.Workers.Playback;
using ScriptCord.Core.Algorithms.Shuffling;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static NHibernate.Loader.Custom.CustomLoader;

namespace ScriptCord.Bot.Services.Playback
{
    public interface IPlaylistService
    {
        Task<Result<LightPlaylistListingDto>> GetPlaylistDetails(ulong guildId, string playlistName, bool isAdmin = false);
        Task<Result<IEnumerable<LightPlaylistListingDto>>> GetPlaylistDetailsByGuildIdAsync(ulong guildId);
        Task<Result> CreateNewPlaylist(ulong guildId, string playlistName, bool isPremiumUser = false);
        Task<Result> RenamePlaylist(ulong guildId, string oldPlaylistName, string newPlaylistName, bool isAdmin = false);
        Task<Result> RemovePlaylist(ulong guildId, string playlistName, bool isAdmin = false);
        Task<Result<IList<PlaylistEntryDto>>> GetShuffledEntries(ulong guildId, string playlistName, bool isAdmin = false);
        Task<Result<AudioMetadataDto>> GetCurrentlyPlayingMetadata(ulong guildId);
        Task<Result<string>> ExportPlaylistToJson(ulong guildId, string playlistName, bool isAdmin = false);
    }

    public class PlaylistService : IPlaylistService
    {
        private readonly ILoggerFacade<IPlaylistService> _logger;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IPlaylistEntriesRepository _playlistEntriesRepository;
        private readonly IConfiguration _configuration;
        private readonly IPlaybackWorker _playbackWorker;

        public PlaylistService(ILoggerFacade<IPlaylistService> logger, 
            IPlaylistRepository playlistRepository, 
            IPlaylistEntriesRepository playlistEntriesRepository, 
            IConfiguration configuration,
            IPlaybackWorker playbackWorker,
            DiscordSocketClient client)
        {
            _logger = logger;
            _logger.SetupDiscordLogging(configuration, client, "playback");

            _playlistRepository = playlistRepository;
            _playlistEntriesRepository = playlistEntriesRepository;
            _configuration = configuration;
            _playbackWorker = playbackWorker;
        }

        public async Task<Result<LightPlaylistListingDto>> GetPlaylistDetails(ulong guildId, string playlistName, bool isAdmin = false)
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
                lastFifteenClips = playlist.PlaylistEntries.OrderByDescending(x => x.UploadTimestamp)
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

        public async Task<Result<IEnumerable<LightPlaylistListingDto>>> GetPlaylistDetailsByGuildIdAsync(ulong guildId)
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

        public async Task<Result> CreateNewPlaylist(ulong guildId, string playlistName, bool isPremiumUser = false)
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
    
        public async Task<Result> RenamePlaylist(ulong guildId, string oldPlaylistName, string newPlaylistName, bool isAdmin = false)
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

        public async Task<Result> RemovePlaylist(ulong guildId, string playlistName, bool isAdmin = false)
        {
            var modelResult = await _playlistRepository.GetSingleAsync(x => x.GuildId == guildId && x.Name == playlistName);
            if (modelResult.IsFailure)
            {
                _logger.LogError(modelResult);
                return Result.Failure("Unexpected error occurred while removing playlist.");
            }
            else if (modelResult.Value == null)
                return Result.Failure("Specified playlist does not exist in this server");

            var model = modelResult.Value;
            if (model.AdminOnly && !isAdmin)
                return Result.Failure("Only an admin can remove this playlist");

            var sourceIdsOfEntriesToRemove = model.PlaylistEntries.AsEnumerable().Select(x => new { SourceIdentifier = x.SourceIdentifier, Source = x.Source });
           

            // Switch first different one to a default playlist if not the only playlist
            if (model.IsDefault)
            {
                var otherPlaylistResult = await _playlistRepository.GetFirstAsync(x => x.GuildId == guildId && x.Name != playlistName);
                if (otherPlaylistResult.IsFailure && otherPlaylistResult.Error != "Sequence contains no elements")
                {
                    _logger.LogError(otherPlaylistResult);
                    return Result.Failure("Unexpected error occurred while counting playlists in the server.");
                }
                else if (otherPlaylistResult.Value != null)
                {
                    var otherModel = otherPlaylistResult.Value;
                    otherModel.IsDefault = true;
                    await _playlistRepository.UpdateAsync(otherModel);
                }
            }
            
            // Remove each entry file that isn't used by other playlists
            foreach(var source in sourceIdsOfEntriesToRemove)
            {
                var isAnyOtherPlaylistUsingResult = await _playlistEntriesRepository
                    .CountAsync(x => x.SourceIdentifier == source.SourceIdentifier && x.Source == source.Source && x.Playlist.Id != model.Id);
                if (isAnyOtherPlaylistUsingResult.IsFailure)
                {
                    _logger.LogError(isAnyOtherPlaylistUsingResult);
                    continue; // This does not concern the user so continue
                }
                else if (isAnyOtherPlaylistUsingResult.Value > 0)
                    continue;

                var strategy = GetStrategyBySource(source.Source);
                var metadata = await strategy.GetMetadataBySourceId(source.SourceIdentifier);
                var baseFolder = _configuration.GetSection("store").GetValue<string>("audioPath");
                var filename = strategy.GenerateFileNameFromMetadata(metadata);
                var filepath = $"./{baseFolder}{filename}.{_configuration.GetSection("store").GetValue<string>("defaultAudioExtension")}";
                try
                {
                    File.Delete(filepath);
                }
                catch (Exception e)
                {
                    _logger.LogException(e); // This error doesn't concern the user so continue
                }
            }

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

            return Result.Success();
        }

        public async Task<Result<IList<PlaylistEntryDto>>> GetShuffledEntries(ulong guildId, string playlistName, bool isAdmin = false)
        {
            var modelResult = await _playlistRepository.GetSingleAsync(x => x.GuildId == guildId && x.Name == playlistName);
            if (modelResult.IsFailure)
            {
                _logger.LogError(modelResult);
                return Result.Failure<IList<PlaylistEntryDto>>("Unexpected error occurred while finding the playlist");
            }
            else if (modelResult.Value == null)
                return Result.Failure<IList<PlaylistEntryDto>>("Specified playlist does not exist in this server");
            else if (modelResult.Value.PlaylistEntries.Count == 0)
                return Result.Failure<IList<PlaylistEntryDto>>("Playlist is empty nothing to shuffle");
            else if (modelResult.Value.AdminOnly && !isAdmin)
                return Result.Failure<IList<PlaylistEntryDto>>("Only an admin can use this playlist");

            var baseFolder = _configuration.GetSection("store").GetValue<string>("audioPath");
            var audioExtension = _configuration.GetSection("store").GetValue<string>("defaultAudioExtension");
            
            IList<PlaylistEntryDto> playlistEntries = modelResult.Value.PlaylistEntries.Select(x =>
            {
                var filename = $"{x.Source}-{x.SourceIdentifier}";
                return new PlaylistEntryDto(x.Id, x.Title, x.AudioLength, $"{baseFolder}{filename}.{audioExtension}");
            }).ToList();
            
            IShuffle<PlaylistEntryDto> shuffler = new FisherYatesListShuffle<PlaylistEntryDto>(playlistEntries);
            shuffler.Shuffle();

            return Result.Success(playlistEntries);
        }

        public async Task<Result<string>> ExportPlaylistToJson(ulong guildId, string playlistName, bool isAdmin = false)
        {
            var modelResult = await _playlistRepository.GetSingleAsync(x => x.GuildId == guildId && x.Name == playlistName);
            if (modelResult.IsFailure)
            {
                _logger.LogError(modelResult);
                return Result.Failure<string>("Unexpected error occurred while finding the playlist");
            }
            else if (modelResult.Value == null)
                return Result.Failure<string>("Specified playlist does not exist in this server");
            else if (modelResult.Value.PlaylistEntries.Count == 0)
                return Result.Failure<string>("Playlist is empty nothing to shuffle");
            else if (modelResult.Value.AdminOnly && !isAdmin)
                return Result.Failure<string>("Only an admin can use this playlist");

            Playlist playlist = modelResult.Value;

            JArray entryArrayJson = new JArray();
            foreach (var entry in playlist.PlaylistEntries)
            {
                entryArrayJson.Add(new JObject(
                    new JProperty("source", entry.Source),
                    new JProperty("sourceIdentifier", entry.SourceIdentifier),
                    new JProperty("title", entry.Title)
                ));
            }

            JObject playlistJson = new JObject(
                new JProperty("name", playlist.Name),
                new JProperty("entries", entryArrayJson)
            );

            return Result.Success(playlistJson.ToString());
        }

        public async Task<Result<AudioMetadataDto>> GetCurrentlyPlayingMetadata(ulong guildId)
        {
            if (!_playbackWorker.HasPlaybackSession(guildId))
                return Result.Failure<AudioMetadataDto>("Currently the bot is not playing any music");

            PlaylistEntryDto entry = _playbackWorker.GetPlaybackSessionData(guildId);

            var playlistEntryResult = await _playlistEntriesRepository.GetSingleAsync(x => x.Id == entry.EntryId);
            if (playlistEntryResult.IsFailure)
            {
                _logger.LogError(playlistEntryResult);
                return Result.Failure<AudioMetadataDto>("Failed to get playlist entry data from the database.");
            }
            var playlistEntry = playlistEntryResult.Value;

            IAudioManagementStrategy strategy = GetStrategyBySource(playlistEntry.Source);
            var metadata = await strategy.GetMetadataBySourceId(playlistEntry.SourceIdentifier);

            // Overwrite title in case it's been updated (should show database info rather than youtube data)
            metadata.Title = playlistEntry.Title;
            return Result.Success(metadata);
        }

        private IAudioManagementStrategy GetStrategyBySource(string source)
        {
            if (source == AudioSourceType.YouTube)
                return new YouTubeAudioManagementStrategy(_configuration);
            else
                throw new NotImplementedException(); // This should never happen
        }
    }
}
