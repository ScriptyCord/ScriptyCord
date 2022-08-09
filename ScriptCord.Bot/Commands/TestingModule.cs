using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Commands
{
    public class TestingModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        [Alias("pong", "hello")]
        public Task PingAsync()
            => ReplyAsync("Pong!");
    }
}
