using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Events
{
    public interface IExecutableEvent
    {
        public ulong GuildId { get; }
    }
}
