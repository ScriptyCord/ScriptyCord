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

        public virtual string SourceIdentifier { get; set; }

        public virtual long AudioLength { get; set; }

        public virtual string AudioLengthFormatted()
        {
            TimeSpan t = TimeSpan.FromMilliseconds(AudioLength);
            string intervalString = null;
            if (AudioLength >= 60)
                intervalString = t.ToString(@"mm\:ss\:fff");
            else
                intervalString = t.ToString(@"ss\:fff");

            return intervalString;
        }

        public virtual Result Validate()
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
