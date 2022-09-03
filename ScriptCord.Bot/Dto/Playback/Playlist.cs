using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Dto.Playback
{
    public class PlaylistListingDto
    {
        public string Name { get; }

        public bool DefaultPlaylist { get; }

        public bool AdminPermission { get; }

        public PlaylistListingDto(string name, bool defaultPlaylist, bool adminPermission)
        {
            Name = name;
            DefaultPlaylist = defaultPlaylist;
            AdminPermission = adminPermission;
        }
    }
}
