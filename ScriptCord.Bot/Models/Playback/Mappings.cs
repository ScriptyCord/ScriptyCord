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
            Table("playlists");
            Id(x => x.Id);
            Map(x => x.GuildId);
            Map(x => x.Name);
            Map(x => x.IsDefault);
            Map(x => x.AdminOnly);
            HasMany(x => x.PlaylistEntries);
        }
    }

    public class PlaylistEntriesMapping : ClassMap<PlaylistEntry>
    {
        public PlaylistEntriesMapping()
        {
            Table("playlist_entries");
            Id(x => x.Id);
            Map(x => x.Title);
            Map(x => x.Source);
            References(x => x.Playlist);
            //HasOne<Playlist>(x => x.Playlist.Id)
        }
    }
}
