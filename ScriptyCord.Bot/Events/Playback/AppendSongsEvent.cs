using ScriptCord.Bot.Dto.Playback;
using ScriptCord.Bot.Events;
using ScriptCord.Bot.Events.Playback;

namespace ScriptyCord.Bot.Events.Playback
{
    public class AppendSongsEvent : PlaybackEventBase, IExecutableEvent
    {
        public IList<PlaylistEntryDto> NewEntries { get; protected set; }
        public ulong GuildId { get; protected set; }

        public AppendSongsEvent(IList<PlaylistEntryDto> newEntries, ulong guildId)
        {
            NewEntries = newEntries;
            GuildId = guildId;
        }
    }
}
