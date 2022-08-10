using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Commands
{
    public interface ITestingModule
    {
        /// <summary>
        /// Responds to a "ping" message with a "pong".
        /// Used for checking bot availability
        /// </summary>
        /// <returns></returns>
        public Task PingAsync();
    }
    
    public class TestingModule : ModuleBase<SocketCommandContext>, ITestingModule
    {
        [Command("ping")]
        [Alias("pong", "hello")]
        public Task PingAsync()
            => ReplyAsync("Pong!");
    }
}
