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
    public class PlaylistEntry : GuidEntity, IModelValidation
    {
        public virtual Playlist Playlist { get; set; }

        public virtual string Title { get; set; }

        public virtual string Source { get; set; }

        public Result Validate()
        {
            if (Title == null || Title.Length == 0)
                return Result.Failure("Title was not supplied");
            else if (Title.Length > 150)
                return Result.Failure("Title can be only 150 characters long");

            if (Source == null || Source.Length == 0)
                return Result.Failure("Source was not supplied");
            else if (Source.Length > 30)
                return Result.Failure("Source can be only 30 characters long");

            return Result.Success();
        }
    }
}
