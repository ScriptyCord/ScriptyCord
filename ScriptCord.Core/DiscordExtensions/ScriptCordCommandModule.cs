using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Core.DiscordExtensions
{
    public abstract class ScriptCordCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        protected bool IsUserGuildAdministrator()
        {
            var guildUser = Context.Guild.Users.FirstOrDefault(x => x.DisplayName == Context.User.Username);
            bool isAdmin;
            if (guildUser == null)
                isAdmin = false;
            else
                isAdmin = guildUser.GuildPermissions.Administrator;
            return isAdmin;
        }
    }
}
