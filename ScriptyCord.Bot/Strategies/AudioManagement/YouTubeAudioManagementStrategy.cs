using AngleSharp.Dom;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using ScriptCord.Bot.Dto.Playback;
using ScriptCord.Bot.Models.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Converter;

namespace ScriptCord.Bot.Strategies.AudioManagement
{
    public class YouTubeAudioManagementStrategy : IAudioManagementStrategy
    {
        private readonly YoutubeClient _client;

        private readonly IConfiguration _configuration;

        public YouTubeAudioManagementStrategy(IConfiguration configuration)
        { 
            _client = new YoutubeClient();
            _configuration = configuration;
        }

        public async Task<Result> DownloadAudio(AudioMetadataDto metadata)
        {
            var filename = GenerateFileNameFromMetadata(metadata);
            var baseFolder = _configuration.GetSection("store").GetValue<string>("audioPath");
            var targetFilename = $"./{baseFolder}{filename}.{_configuration.GetSection("store").GetValue<string>("defaultAudioExtension")}";

            try
            {
                await _client.Videos.DownloadAsync(metadata.Url, targetFilename);
            }
            catch (Exception e)
            {
                return Result.Failure(e.Message);
            }

            return Result.Success();
        }

        public async Task<AudioMetadataDto> ExtractMetadataFromUrl(string url)
        {
            var video = await _client.Videos.GetAsync(url);
            return new AudioMetadataDto
            {
                Title = video.Title,
                AudioLength = (long)video.Duration.Value.TotalMilliseconds,
                Thumbnail = video.Thumbnails.OrderByDescending(x => x.Resolution.Width * x.Resolution.Height).First().Url,
                SourceType = AudioSourceType.YouTube,
                SourceId = video.Id,
                Url = url
            };
        }

        public async Task<InternetPlaylistMetadataDto> ExtractPlaylistMetadata(string playlistUrl)
        {
            var playlist = await _client.Playlists.GetAsync(playlistUrl);

            InternetPlaylistMetadataDto dto = new InternetPlaylistMetadataDto();
            dto.Title = playlist.Title;
            IList<AudioMetadataDto> entries = new List<AudioMetadataDto>();

            var videos = await _client.Playlists.GetVideosAsync(playlistUrl);
            foreach (var entry in videos)
            {
                entries.Add(await ExtractMetadataFromUrl(entry.Url));
            }

            dto.Entries = entries;
            return dto;
        }

        public string GenerateFileNameFromMetadata(AudioMetadataDto metadata)
            => $"{metadata.SourceType}-{metadata.SourceId}";

        public async Task<AudioMetadataDto> GetMetadataBySourceId(string sourceId)
        {
            var video = await _client.Videos.GetAsync(sourceId, default(CancellationToken));
            return new AudioMetadataDto
            {
                Title = video.Title,
                AudioLength = (long)video.Duration.Value.TotalMilliseconds,
                Thumbnail = video.Thumbnails.OrderByDescending(x => x.Resolution.Width * x.Resolution.Height).First().Url,
                SourceType = AudioSourceType.YouTube,
                SourceId = video.Id,
                Url = video.Url
            };
        }
    }
}
