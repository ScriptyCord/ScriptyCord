using AngleSharp.Dom;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using ScriptCord.Bot.Dto.Playback;
using ScriptCord.Bot.Models.Playback;
using ScriptCord.Bot.Repositories.Playback;
using ScriptCord.Bot.Strategies.AudioManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Services.Playback
{
    public interface IPlaylistEntriesService
    {
        Task<Result<AudioMetadataDto>> AddEntryFromUrlToPlaylistByName(ulong guildId, string playlistName, string url, bool isAdmin = false);
        Task<Result<AudioMetadataDto>> RemoveEntryFromPlaylistByName(ulong guildId, string playlistName, string entryName, bool isAdmin = false);
    }

    public class PlaylistEntriesService : IPlaylistEntriesService
    {
        private readonly IPlaylistEntriesRepository _playlistEntriesRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly ILoggerFacade<IPlaylistEntriesService> _logger;
        private readonly IConfiguration _configuration;

        public PlaylistEntriesService(ILoggerFacade<IPlaylistEntriesService> logger, IPlaylistRepository playlistRepository, IPlaylistEntriesRepository playlistEntriesRepository, IConfiguration configuration)
        {
            _playlistEntriesRepository = playlistEntriesRepository;
            _playlistRepository = playlistRepository;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<Result<AudioMetadataDto>> AddEntryFromUrlToPlaylistByName(ulong guildId, string playlistName, string url, bool isAdmin = false)
        {
            var playlistResult = await _playlistRepository.GetSingleAsync(x => x.GuildId == guildId && x.Name == playlistName);
            if (playlistResult.IsFailure)
            {
                _logger.LogError(playlistResult);
                return Result.Failure<AudioMetadataDto>($"Unable to find playlist named {playlistName}");
            }
            var playlist = playlistResult.Value;
            if (playlist.AdminOnly && !isAdmin)
                return Result.Failure<AudioMetadataDto>($"You must be the administrator of the guild to add an entry to this playlist");

            Result<IAudioManagementStrategy> strategyResult = GetSuitableStrategyByUrl(url);
            if (strategyResult.IsFailure)
                return Result.Failure<AudioMetadataDto>(strategyResult.Error);
            IAudioManagementStrategy strategy = strategyResult.Value;
            AudioMetadataDto metadata = await strategy.ExtractMetadataFromUrl(url);

            var checkIfAlreadyDownloadedResult = await _playlistEntriesRepository.CountAsync(x => x.SourceIdentifier == metadata.SourceId);
            if (checkIfAlreadyDownloadedResult.IsFailure)
            {
                _logger.LogError(checkIfAlreadyDownloadedResult);
                return Result.Failure<AudioMetadataDto>($"Unexpected error occurred when trying to check if file already downloaded");
            }
            if (checkIfAlreadyDownloadedResult.Value == 0)
            {
                // TODO: Trigger download event instead
                Result audioDownloadResult = await strategy.DownloadAudio(metadata);
                if (audioDownloadResult.IsFailure)
                {
                    _logger.LogError(audioDownloadResult);
                    return Result.Failure<AudioMetadataDto>(audioDownloadResult.Error);
                }
            }
            if (playlist.PlaylistEntries.Count(x => x.SourceIdentifier == metadata.SourceId) == 0)
            {
                PlaylistEntry newEntry = new PlaylistEntry { Playlist = playlist, UploadTimestamp = DateTime.UtcNow, Title = metadata.Title, Source = metadata.SourceType, SourceIdentifier = metadata.SourceId, AudioLength = metadata.AudioLength };
                var result = await _playlistEntriesRepository.SaveAsync(newEntry);
                if (result.IsFailure)
                {
                    _logger.LogError(result);
                    return Result.Failure<AudioMetadataDto>(result.Error);
                }
            }

            return Result.Success(metadata);
        }

        public async Task<Result<AudioMetadataDto>> RemoveEntryFromPlaylistByName(ulong guildId, string playlistName, string entryName, bool isAdmin = false)
        {
            var playlistResult = await _playlistRepository.GetSingleAsync(x => x.GuildId == guildId && x.Name == playlistName);
            if (playlistResult.IsFailure)
            {
                _logger.LogError(playlistResult);
                return Result.Failure<AudioMetadataDto>($"Unable to find playlist named {playlistName}");
            }
            var playlist = playlistResult.Value;
            if (playlist.AdminOnly && !isAdmin)
                return Result.Failure<AudioMetadataDto>($"You must be the administrator of the guild to remove an entry from this playlist");

            PlaylistEntry entry = null;
            try
            {
                entry = playlist.PlaylistEntries.First(x => x.Title == entryName);
            }
            catch (Exception e)
            {
                return Result.Failure<AudioMetadataDto>($"Didn't find an entry named '{entryName}' in playlist '{playlistName}'");
            }

            var strategyValue = GetSuitableStrategyBySource(entry.Source);
            if (strategyValue.IsFailure)
                return Result.Failure<AudioMetadataDto>(strategyValue.Error);

            IAudioManagementStrategy strategy = strategyValue.Value;
            var metadata = await strategy.GetMetadataBySourceId(entry.SourceIdentifier);
            var baseFolder = _configuration.GetSection("store").GetValue<string>("audioPath");
            var filename = strategy.GenerateFileNameFromMetadata(metadata);
            var filepath = $"{baseFolder}{filename}.{_configuration.GetSection("store").GetValue<string>("defaultAudioExtension")}";

            // TODO: Trigger event to delete if no other guild uses this file too and continue the below only on success

            playlist.PlaylistEntries.Remove(entry);
            Result removeResult = await _playlistEntriesRepository.DeleteAsync(entry);
            if (removeResult.IsFailure)
            {
                _logger.LogError(removeResult);
                return Result.Failure<AudioMetadataDto>("Unexpected error while removing an entry from playlist");
            }

            //Result result = await _playlistRepository.UpdateAsync(playlist);
            //if (result.IsFailure)
            //{
            //    _logger.LogError(result);
            //    return Result.Failure<AudioMetadataDto>("Unexpected error while removing an entry from playlist");
            //}

            return Result.Success(metadata);
        }

        private Result<IAudioManagementStrategy> GetSuitableStrategyByUrl(string url)
        {
            IAudioManagementStrategy audioManagementStrategy = null;
            if (url.StartsWith("https://www.youtube.com/watch") || url.StartsWith("https://youtu.be/"))
                audioManagementStrategy = new YouTubeAudioManagementStrategy(_configuration);
            else
            {
                _logger.LogDebug($"No suitable strategy was found for url: {url}");
                return Result.Failure<IAudioManagementStrategy>($"Unable to extract audio from url: {url}.");
            }

            return Result.Success(audioManagementStrategy);
        }

        private Result<IAudioManagementStrategy> GetSuitableStrategyBySource(string source)
        {
            IAudioManagementStrategy audioManagementStrategy = null;
            if (source == AudioSourceType.YouTube) 
                audioManagementStrategy = new YouTubeAudioManagementStrategy(_configuration);
            else
            {
                _logger.LogDebug($"No suitable strategy was found for source: {source}");
                return Result.Failure<IAudioManagementStrategy>($"Unable to find strategy for: {source}.");
            }

            return Result.Success(audioManagementStrategy);
        }
    }
}
