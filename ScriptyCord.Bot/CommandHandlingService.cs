using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot
{
    public interface ICommandHandlingService
    {
        Task InitializeAsync();
        Task MessageReceivedAsync(SocketMessage rawMessage);
        Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result);
    }

    public class CommandHandlingService : ICommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFacade<ICommandHandlingService> _logger;

        public CommandHandlingService(CommandService commands, DiscordSocketClient discord, IServiceProvider serviceProvider, ILoggerFacade<ICommandHandlingService> logger)
        {
            _commands = commands;
            _discord = discord;
            _serviceProvider = serviceProvider;
            _logger = logger;

            _commands.CommandExecuted += CommandExecutedAsync;
            _discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
            => await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // TODO: This first if possibly could be only 
            // in production. Automated tests via another bot
            // might be implemented in the future by a tester.
            if (!(rawMessage is SocketUserMessage message))
                return;
            if (message.Source != MessageSource.User)
                return;

            var argpos = 0;

            if (!message.HasMentionPrefix(_discord.CurrentUser, ref argpos))
                return;

            var context = new SocketCommandContext(_discord, message);
            await _commands.ExecuteAsync(context, argpos, _serviceProvider);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
                return;

            if (result.IsSuccess)
                return;

            await context.Channel.SendMessageAsync($"error: {result}");
        }
    }
}
