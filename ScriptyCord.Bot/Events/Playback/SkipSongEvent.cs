using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Events.Playback
{
    public class SkipSongEvent : PlaybackEventBase, IExecutableEvent
    {
        public ulong GuildId { get; protected set; }

        public SkipSongEvent(ulong guildId)
        {
            GuildId = guildId;
        }
    }
}
