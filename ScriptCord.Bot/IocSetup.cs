using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ScriptCord.Bot.Commands;
using ScriptCord.Bot.Repositories;
using ScriptCord.Bot.Services.Playback;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot
{
    public class IocSetup
    {
        private ServiceCollection _services;

        public IocSetup(IConfiguration configuration)
        {
            _services = new ServiceCollection();
            
            _services.AddSingleton(new DiscordSocketConfig());
            _services.AddSingleton(configuration);
            _services.AddSingleton<DiscordSocketClient>();
            _services.AddSingleton<InteractionHandler>();
            _services.AddScoped(typeof(LoggerFacade<>));
        }

        public void SetupRepositories(IConfiguration config)
        {
            var connectionString = config.GetSection("ConnectionStrings").GetSection("DefaultConnection").Get<string>();
            IDbConnection db = new NpgsqlConnection(connectionString);
            _services.AddSingleton<IScriptCordUnitOfWork>(new ScriptCordUnitOfWork(db));
        }

        public void SetupServices()
        {
            _services.AddScoped<IPlaylistService, PlaylistService>();
        }

        public void SetupCommandModules()
        {
            _services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
        }

        public ServiceProvider Build()
            => _services.BuildServiceProvider();
    }
}
