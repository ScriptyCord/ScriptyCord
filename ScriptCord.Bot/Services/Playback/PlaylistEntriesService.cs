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
        Task<Result<AudioMetadataDto>> AddEntryFromUrlToPlaylistByName(long guildId, string playlistName, string url);
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

        public async Task<Result<AudioMetadataDto>> AddEntryFromUrlToPlaylistByName(long guildId, string playlistName, string url)
        {
            var playlistResult = await _playlistRepository.GetSingleAsync(x => x.GuildId == guildId && x.Name == playlistName);
            if (playlistResult.IsFailure)
            {
                _logger.LogError(playlistResult);
                return Result.Failure<AudioMetadataDto>($"Unable to find playlist named {playlistName}");
            }
            var playlist = playlistResult.Value;

            Result<IAudioManagementStrategy> strategyResult = GetSuitableStrategy(url);
            if (strategyResult.IsFailure)
                return Result.Failure<AudioMetadataDto>(strategyResult.Error);
            IAudioManagementStrategy strategy = strategyResult.Value;
            AudioMetadataDto metadata = await strategy.ExtractMetadataFromUrl(url);

            if (playlist.PlaylistEntries.Count(x => x.SourceIdentifier == metadata.SourceId) > 0)
                return Result.Success(metadata);
            
            PlaylistEntry newEntry = new PlaylistEntry { Playlist = playlist, Title = metadata.Title, Source = metadata.SourceType, AudioLength = metadata.AudioLength };

            Result audioDownloadResult = await strategy.DownloadAudio(metadata);
            if (audioDownloadResult.IsFailure)
            {
                _logger.LogError(audioDownloadResult);
                return Result.Failure<AudioMetadataDto>(audioDownloadResult.Error);
            }

            var result = await _playlistEntriesRepository.SaveAsync(newEntry);
            if (result.IsFailure)
            {
                _logger.LogError(result);
                return Result.Failure<AudioMetadataDto>(result.Error);
            }

            return Result.Success(metadata);
        }

        private Result<IAudioManagementStrategy> GetSuitableStrategy(string url)
        {
            IAudioManagementStrategy audioManagementStrategy = null;
            if (url.Contains("youtube") || url.Contains("youtu.be")) // TODO: Better pattern matching
                audioManagementStrategy = new YouTubeAudioManagementStrategy(_configuration);
            else
            {
                _logger.LogDebug($"No suitable strategy was found for url: {url}");
                return Result.Failure<IAudioManagementStrategy>($"Unable to extract audio from url: {url}.");
            }

            return Result.Success(audioManagementStrategy);
        }
    }
}
