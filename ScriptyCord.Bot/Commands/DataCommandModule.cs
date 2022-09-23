using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using ScriptCord.Bot;
using ScriptCord.Core.DiscordExtensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScriptyCord.Bot.Commands
{
    public class DataCommandModule : ScriptCordCommandModule
    {
        private new readonly Discord.Color _modulesEmbedColor = Discord.Color.DarkPurple;
        private readonly ILoggerFacade<DataCommandModule> _logger;

        public DataCommandModule(ILoggerFacade<DataCommandModule> logger, DiscordSocketClient client, IConfiguration configuration)
        {
            _logger = logger;
            _logger.SetupDiscordLogging(configuration, client, "general");
        }

        [SlashCommand("instance-info", "Displays info about the version and build of this bot's instance")]
        public async Task InstanceInfo()
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Showing instance info");
            DateTime buildTime = GetLinkerTime(Assembly.GetEntryAssembly());
            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithColor(_modulesEmbedColor)
                .WithTitle($"ScriptyCord Version: {Program.Version}")
                .WithDescription($"Running in '{Environment.GetEnvironmentVariable("ENVIRONMENT_TYPE")}' environment on {RuntimeInformation.OSArchitecture} {RuntimeInformation.OSDescription}, built at {buildTime}.");

            await RespondAsync(embed: embedBuilder.Build());
        }

        public DateTime GetLinkerTime(Assembly assembly)
        {
            const string BuildVersionMetadataPrefix = "+build";

            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0)
                {
                    value = value[(index + BuildVersionMetadataPrefix.Length)..];
                    return DateTime.ParseExact(value, "yyyy-MM-ddTHH:mm:ss:fffZ", CultureInfo.InvariantCulture);
                }
            }

            return default;
        }
    }
}
