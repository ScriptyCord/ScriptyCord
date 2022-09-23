using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Dto.Playback
{
    public class LightPlaylistEntryDto
    {
        public string Title { get; set; }

        public string Id { get; set; }

        public string Source { get; set; }

        public string SourceId { get; set; }

        public string AudioLength { get; set; }

        public string UploadedAt { get; set; }
    }
}
