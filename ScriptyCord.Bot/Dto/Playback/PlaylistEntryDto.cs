using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Dto.Playback
{
    public class PlaylistEntryDto
    {
        public Guid EntryId { get; protected set; }

        public string Title { get; protected set; }
        
        public long Length { get; protected set; }

        public string Path { get; protected set; }

        public PlaylistEntryDto(Guid entryId, string title, long length, string path)
        {
            EntryId = entryId;
            Title = title;
            Length = length;
            Path = path;
        }
    }
}
