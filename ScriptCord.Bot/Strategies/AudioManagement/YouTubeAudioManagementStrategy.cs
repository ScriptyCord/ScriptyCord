using CSharpFunctionalExtensions;
using ScriptCord.Bot.Dto.Playback;
using ScriptCord.Bot.Models.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;

namespace ScriptCord.Bot.Strategies.AudioManagement
{
    public class YouTubeAudioManagementStrategy : IAudioManagementStrategy
    {
        private readonly YoutubeClient _client;

        public YouTubeAudioManagementStrategy()
            => _client = new YoutubeClient();

        public Result DownloadAudioFromUrl(string url)
        {
            return Result.Failure("");
        }

        public async Task<AudioMetadataDto> ExtractMetadataFromUrl(string url)
        {
            var video = await _client.Videos.GetAsync(url);
            return new AudioMetadataDto
            {
                Title = video.Title,
                AudioLength = (long)video.Duration.Value.TotalSeconds,
                Thumbnail = video.Thumbnails.OrderByDescending(x => x.Resolution.Width * x.Resolution.Height).First().Url,
                SourceType = AudioSourceType.YouTube
            };
        }

        public string GenerateFileNameFromModel(PlaylistEntry entry)
        {
            return "";
        }
    }
}
