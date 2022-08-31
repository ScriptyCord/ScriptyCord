using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Models.Playback
{
    [Table("playlists", Schema = "scriptcord")]
    public class Playlist
    {
        [Column("id", Order = 0)]
        public int Id { get; set; }

        [Column("guild_id", Order = 1)]
        public long GuildId { get; set; }

        [Column("name", Order = 2)]
        public string Name { get; set; }

        [Column("is_default", Order = 3)]
        public bool IsDefault { get; set; }

        [Column("admin_only", Order = 4)]
        public bool AdminOnly { get; set; }
    }
}
