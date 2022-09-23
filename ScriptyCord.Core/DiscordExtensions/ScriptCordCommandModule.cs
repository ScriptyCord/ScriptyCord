using Discord;
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
        //protected readonly Discord.Color _modulesEmbedColor = Discord.Color.Teal;
        protected readonly Discord.Color _modulesEmbedColor = Discord.Color.DarkGreen;

        protected Embed CommandIsBeingProcessedEmbed(string groupName, string commandName, string processingMessage = "Command is being processed. Please wait...")
            => new EmbedBuilder().WithColor(_modulesEmbedColor).WithTitle($"{groupName} {commandName}").WithDescription(processingMessage).Build();

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
