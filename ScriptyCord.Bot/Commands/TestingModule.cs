using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using ScriptCord.Core.DiscordExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot.Commands
{
#if DEBUG
    public class TestingModule : ScriptyCordCommandModule
    {
        private ILoggerFacade<TestingModule> _logger;

        public TestingModule(ILoggerFacade<TestingModule> logger, DiscordSocketClient client, IConfiguration configuration)
        {
            _logger = logger;
            _logger.SetupDiscordLogging(configuration, client, "general");
        }

        [SlashCommand("echo", "Echo an input")]
        public async Task Echo(string echo, [Summary(description: "mention the user")] bool mention = false)
        {
            _logger.LogDebug($"[GuildId({Context.Guild.Id}),ChannelId({Context.Channel.Id})]: Echoing a user message");
            await RespondAsync(echo + (mention ? Context.User.Mention : string.Empty));
        }

        [SlashCommand("foo-bar", "Dummy command to execute any code the developer puts below")]
        public async Task FooBar()
        {
            _logger.LogFatalException(new Exception("Test an exception"));
            await RespondAsync("Foo and bar");
        }
    }
#endif
}
