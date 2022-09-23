using CSharpFunctionalExtensions;
using ScriptCord.Core.Persistency;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Models.Playback
{
    public class Playlist : LongEntity, IModelValidation
    {
        public virtual ulong GuildId { get; set; }

        public virtual string Name { get; set; }

        public virtual bool IsDefault { get; set; }

        public virtual bool AdminOnly { get; set; }

        public virtual IList<PlaylistEntry> PlaylistEntries { get; set; } = new List<PlaylistEntry>();

        public virtual Result Validate()
        {
            if (Name == null || Name.Length == 0)
                return Result.Failure("The playlist name was not supplied");
            else if(Name.Length > 80)
                return Result.Failure("Playlist name length can be only 80 characters long");
             

            return Result.Success();
        }
    }
}
