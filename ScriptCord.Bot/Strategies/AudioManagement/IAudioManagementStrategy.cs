﻿using CSharpFunctionalExtensions;
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
        string GenerateFileNameFromModel(AudioMetadataDto metadata);
        Task<Result> DownloadAudio(AudioMetadataDto metadata);
        Task<AudioMetadataDto> ExtractMetadataFromUrl(string url);
    }

    public static class AudioSourceType
    {
        public static readonly string YouTube = "YouTube";
    };
}
