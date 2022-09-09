using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Dto.Playback
{
    public class AudioMetadataDto
    {
        public string Title { get; set; }

        public long AudioLength { get; set; }

        public string Thumbnail { get; set; }

        public string SourceType { get; set; }

        public string SourceId { get; set; }

        public string Url { get; set; }
    }
}
