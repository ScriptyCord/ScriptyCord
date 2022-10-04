using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using ScriptCord.Bot;
using ScriptCord.Bot.Workers.Playback;
using ScriptCord.Core.DiscordExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly PlaybackWorker _playbackWorker;

        public DataCommandModule(ILoggerFacade<DataCommandModule> logger, DiscordSocketClient client, IConfiguration configuration, PlaybackWorker playbackWorker)
        {
            _logger = logger;
            _logger.SetupDiscordLogging(configuration, client, "general");
            _playbackWorker = playbackWorker;
        }

        [SlashCommand("instance-info", "Displays info about the version and build of this bot's instance")]
        public async Task InstanceInfo()
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Showing instance info");
            DateTime buildTime = GetLinkerTime(Assembly.GetEntryAssembly());

            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            fields.Add(new EmbedFieldBuilder().WithName("Environment Type").WithValue(Environment.GetEnvironmentVariable("ENVIRONMENT_TYPE")));
            fields.Add(new EmbedFieldBuilder().WithName("Architecture").WithValue(RuntimeInformation.OSArchitecture));
            fields.Add(new EmbedFieldBuilder().WithName("Operating System").WithValue(RuntimeInformation.OSDescription));
            fields.Add(new EmbedFieldBuilder().WithName("Built at").WithValue($"{buildTime} UTC"));
            fields.Add(new EmbedFieldBuilder().WithName("Running for").WithValue(DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()));
            fields.Add(new EmbedFieldBuilder().WithName("Server count").WithValue(Context.Client.Guilds.Count));
            fields.Add(new EmbedFieldBuilder().WithName("Active voice sessions").WithValue(_playbackWorker.GetPlaybackSessionsCount()));

            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithColor(_modulesEmbedColor)
                .WithTitle($"ScriptyCord Version: {Program.Version}")
                .WithImageUrl(Context.Client.CurrentUser.GetAvatarUrl())
                .WithFields(fields);

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
