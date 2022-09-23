using Discord;
using Discord.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Events.Playback
{
    public class PlaybackEventBase
    {
        public IAudioClient Client { get; protected set;  }

        public IVoiceChannel Channel { get; protected set; }

        public PlaybackEventBase(IAudioClient client, IVoiceChannel channel)
        {
            Client = client;
            Channel = channel;
        }

        public PlaybackEventBase() { }
    }
}
