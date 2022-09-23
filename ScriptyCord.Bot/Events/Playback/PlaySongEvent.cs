using CSharpFunctionalExtensions;
using Discord;
using Discord.Audio;
using ScriptCord.Bot.Dto.Playback;
using ScriptCord.Bot.Workers.Playback;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Events.Playback
{
    public class PlaySongEvent : PlaybackEventBase, IExecutableEvent
    {
        public IList<PlaylistEntryDto> Playlist { get; protected set; }

        public ulong GuildId { get; protected set; }

        public PlaySongEvent(IAudioClient client, IVoiceChannel channel, ulong guildId, IList<PlaylistEntryDto> playlist) : base(client, channel) 
        {
            Playlist = playlist;
            GuildId = guildId;
        }
    }
}
