using Discord;
using Discord.Interactions;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Commands
{
    public class UserManagementModule : InteractionModuleBase<SocketInteractionContext>
    {
        //private readonly ILoggerFacade<UserManagementModule> _logger;

        //public UserManagementModule(ILoggerFacade<UserManagementModule> logger)
        //    => _logger = logger;

        [SlashCommand("ban", "Bans the specified user from the server")]
        public Task Ban([Summary(description: "User that will be affected by this command")] IGuildUser user)
            => throw new NotImplementedException();

        //[SlashCommand("Unban", "")]
        //public Task Unban()
        //    => throw new NotImplementedException();


        [SlashCommand("kick", "Kicks the specified user from the server")]
        public Task Kick([Summary(description: "User that will be affected by this command")] IGuildUser user)
            => throw new NotImplementedException();
    }
}
