using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Models.Playback
{
    public class PlaylistMapping : ClassMap<Playlist>
    {
        public PlaylistMapping()
        {
            Schema("scriptycord");
            Table("playlists");
            Id(x => x.Id).Column("id");
            Map(x => x.GuildId).Column("guild_id");
            Map(x => x.Name).Column("name");
            Map(x => x.IsDefault).Column("is_default");
            Map(x => x.AdminOnly).Column("admin_only");
            HasMany(x => x.PlaylistEntries).Cascade.SaveUpdate();
        }
    }

    public class PlaylistEntriesMapping : ClassMap<PlaylistEntry>
    {
        public PlaylistEntriesMapping()
        {
            Schema("scriptycord");
            Table("playlist_entries");
            Id(x => x.Id).Column("id").GeneratedBy.GuidComb();
            Map(x => x.Title).Column("title");
            Map(x => x.Source).Column("source");
            Map(x => x.AudioLength).Column("audio_length");
            References(x => x.Playlist).Cascade.SaveUpdate();
            //HasOne<Playlist>(x => x.Playlist.Id)
        }
    }
}
