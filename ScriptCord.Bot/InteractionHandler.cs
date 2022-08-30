﻿using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ScriptCord.Bot
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private readonly LoggerFacade<InteractionHandler> _logger;

        public InteractionHandler(DiscordSocketClient client, InteractionService interactionService, IServiceProvider services, IConfiguration config, LoggerFacade<InteractionHandler> logger)
        {
            _client = client;
            _interactionService = interactionService;
            _services = services;
            _configuration = config;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            _client.Ready += ReadyAsync;
            _interactionService.Log += LogAsync;
        }

        private async Task LogAsync(LogMessage log)
            => await _logger.LogAsync(log);

        private async Task ReadyAsync()
        {
            try
            {
                await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            }
            catch (Exception e) 
            {
                _logger.Log(NLog.LogLevel.Fatal, e.Message);
                _client.Dispose();
                System.Environment.Exit(1);
            }
            await _interactionService.RegisterCommandsGloballyAsync(true);
            _client.InteractionCreated += HandleInteraction;
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_client, interaction);
                var result = await _interactionService.ExecuteCommandAsync(context, _services);

                if (!result.IsSuccess)
                {
                    switch (result.Error)
                    {
                        case InteractionCommandError.UnmetPrecondition:
                            // TODO
                            break;
                        default:
                            break;
                    }
                }
                    
            }
            catch
            {
                // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if (interaction.Type is InteractionType.ApplicationCommand)
                    await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}
