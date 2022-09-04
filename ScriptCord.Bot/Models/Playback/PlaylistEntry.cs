using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Models.Playback
{
    [Table("playlist_entries", Schema = "scriptycord")]
    public class PlaylistEntry : IModelValidation
    {
        [Column("id", Order = 0)]
        public Guid Id { get; set; }

        [Column("playlist_id", Order = 1)]
        public int PlaylistId { get; set; }

        [Column("title", Order = 2)]
        public string Title { get; set; }

        [Column("source", Order = 3)]
        public string Source { get; set; }

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
