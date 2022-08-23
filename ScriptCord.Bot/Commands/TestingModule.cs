using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Commands
{
    public class TestingModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly InteractionService _commands;

        private readonly InteractionHandler _handler;

        public TestingModule(InteractionService commands, InteractionHandler handler)
        {
            _commands = commands;
            _handler = handler;
        }

        [SlashCommand("echo", "Echo an input")]
        public async Task Echo(string echo, [Summary(description: "mention the user")] bool mention = false)
            => await RespondAsync(echo + (mention ? Context.User.Mention : string.Empty));
    }
}
