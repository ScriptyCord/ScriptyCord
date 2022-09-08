using AngleSharp.Dom;
using CSharpFunctionalExtensions;
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
        Task<Result<AudioMetadataDto>> AddEntryFromUrlToPlaylistByName(long guildId, string playlist, string url);
    }

    public class PlaylistEntriesService : IPlaylistEntriesService
    {
        private readonly IPlaylistEntriesRepository _playlistEntriesRepository;
        private readonly ILoggerFacade<IPlaylistEntriesService> _logger;

        public PlaylistEntriesService(ILoggerFacade<IPlaylistEntriesService> logger, IPlaylistEntriesRepository playlistEntriesRepository)
        {
            _playlistEntriesRepository = playlistEntriesRepository;
            _logger = logger;
        }

        public async Task<Result<AudioMetadataDto>> AddEntryFromUrlToPlaylistByName(long guildId, string playlist, string url)
        {
            Result<IAudioManagementStrategy> strategyResult = GetSuitableStrategy(url);
            if (strategyResult.IsFailure)
                return Result.Failure<AudioMetadataDto>(strategyResult.Error);

            IAudioManagementStrategy strategy = strategyResult.Value;

            AudioMetadataDto metadata = await strategy.ExtractMetadataFromUrl(url);
            PlaylistEntry newEntry = new PlaylistEntry { Playlist = new Playlist { Id = guildId }, Title = metadata.Title, Source = metadata.SourceType, AudioLength = metadata.AudioLength };
            var filename = strategy.GenerateFileNameFromModel(newEntry);

            // TODO save to disk and only continue if successfull

            return Result.Success(metadata);
        }

        private Result<IAudioManagementStrategy> GetSuitableStrategy(string url)
        {
            IAudioManagementStrategy audioManagementStrategy = null;
            if (url.Contains("youtube") || url.Contains("youtu.be")) // TODO: Better pattern matching
                audioManagementStrategy = new YouTubeAudioManagementStrategy();
            else
            {
                _logger.LogDebug($"No suitable strategy was found for url: {url}");
                return Result.Failure<IAudioManagementStrategy>($"Unable to extract audio from url: {url}.");
            }

            return Result.Success(audioManagementStrategy);
        }
    }
}
