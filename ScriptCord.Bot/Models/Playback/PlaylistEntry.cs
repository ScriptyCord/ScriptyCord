using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Models.Playback
{
    [Table("playlist_entries", Schema = "scriptycord")]
    public class PlaylistEntry
    {
        [Column("id", Order = 0)]
        public Guid Id { get; set; }

        [Column("playlist_id", Order = 1)]
        public int PlaylistId { get; set; }

        [Column("title", Order = 2)]
        public string Title { get; set; }

        [Column("source", Order = 3)]
        public string Source { get; set; }
    }
}
