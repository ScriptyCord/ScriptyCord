﻿using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Models.Playback
{
    [Table("playlists", Schema = "scriptycord")]
    public class Playlist : IModelValidation
    {
        [Column("id", Order = 0)]
        public int Id { get; set; }

        [Column("guild_id", Order = 1)]
        public long GuildId { get; set; }

        [Column("name", Order = 2)]
        public string Name { get; set; }

        [Column("is_default", Order = 3)]
        public bool IsDefault { get; set; }

        [Column("admin_only", Order = 4)]
        public bool AdminOnly { get; set; }

        public Result Validate()
        {
            if (Name.Length > 80)
                return Result.Failure("Playlist name length can be only 80 characters long");
            else if (Name == null || Name.Length == 0)
                return Result.Failure("The playlist name was not supplied");

            return Result.Success();
        }
    }
}
