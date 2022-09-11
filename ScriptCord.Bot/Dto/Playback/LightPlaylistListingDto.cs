using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Dto.Playback
{
    public class LightPlaylistListingDto
    {
        public string Name { get; protected set; }

        public bool DefaultPlaylist { get; protected set; }

        public bool AdminPermission { get; protected set; }

        public int SongCount { get; protected set; }

        public LightPlaylistListingDto(string name, bool defaultPlaylist, bool adminPermission, int songCount)
        {
            Name = name;
            DefaultPlaylist = defaultPlaylist;
            AdminPermission = adminPermission;
            SongCount = songCount;
        }
    }
}
