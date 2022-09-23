using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Dto.Playback
{
    public class DetailedPlaylistDto
    {
        public string Name { get; protected set; }

        public bool IsDefault { get; protected set; }
    
        public bool IsAdmin { get; protected set; }

        public ulong TotalDuration { get; protected set; }

        public IEnumerable<AudioMetadataDto> Metadata { get; protected set; }

        public DetailedPlaylistDto(string name, bool isDefault, bool isAdmin, IEnumerable<AudioMetadataDto> metadata)
        {
            Name = name;
            IsDefault = isDefault;
            IsAdmin = isAdmin;
            Metadata = metadata;
            TotalDuration = Metadata.Aggregate(0UL, (a, c) => a + (ulong)c.AudioLength);
        }
    }
}
