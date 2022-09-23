using CSharpFunctionalExtensions;
using Discord;
using Discord.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Events.Playback
{
    public class StopPlaybackEvent : PlaybackEventBase, IExecutableEvent
    {
        public ulong GuildId { get; protected set; }

        public StopPlaybackEvent(ulong guildId)
            => GuildId = guildId;
    }
}
