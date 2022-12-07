using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Core.DiscordExtensions
{
    public abstract class ScriptyCordCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        protected Embed CommandIsBeingProcessedEmbed(string groupName, string commandName, string processingMessage = "Command is being processed. Please wait...")
            => new EmbedBuilder().WithColor(Discord.Color.Blue).WithTitle($"{groupName} {commandName}").WithDescription(processingMessage).Build();

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
