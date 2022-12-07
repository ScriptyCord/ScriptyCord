using CSharpFunctionalExtensions;
using ScriptCord.Bot.Dto.Playback;
using ScriptCord.Bot.Models.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Strategies.AudioManagement
{
    public interface IAudioManagementStrategy
    {
        string GenerateFileNameFromMetadata(AudioMetadataDto metadata);
        Task<Result> DownloadAudio(AudioMetadataDto metadata);
        Task<Result<AudioMetadataDto>> ExtractMetadataFromUrl(string url);
        Task<Result<InternetPlaylistMetadataDto>> ExtractPlaylistMetadata(string playlistUrl);
        Task<AudioMetadataDto> GetMetadataBySourceId(string sourceId);
    }

    public static class AudioSourceType
    {
        public static readonly string YouTube = "YouTube";
    };
}
