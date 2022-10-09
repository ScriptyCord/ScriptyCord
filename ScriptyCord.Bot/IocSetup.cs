using Discord.Interactions;
using Discord.WebSocket;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.Dialect;
using Npgsql;
using ScriptCord.Bot.Commands;
using ScriptCord.Bot.Models.Playback;
using ScriptCord.Bot.Repositories;
using ScriptCord.Bot.Repositories.Playback;
using ScriptCord.Bot.Services.Playback;
using ScriptCord.Bot.Workers.Playback;
using ScriptCord.Core.Persistency;
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

        private IConfiguration _configuration;

        private ISessionFactory NHibernateSessionFactory
        {
            get
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");


                return Fluently.Configure()
                    .Database(PostgreSQLConfiguration.Standard.ConnectionString(connectionString).Dialect<PostgreSQL82Dialect>())
                    .Mappings(m =>
                    {
                        m.FluentMappings.AddFromAssemblyOf<PlaylistMapping>();
                        m.FluentMappings.AddFromAssemblyOf<PlaylistEntriesMapping>();
                    }).BuildSessionFactory();
                //.ExposeConfiguration(TreatConfiguration)
            }
        }

        public IocSetup(IConfiguration configuration)
        {
            _services = new ServiceCollection();
            _configuration = configuration;

            _services.AddSingleton(new DiscordSocketConfig());
            _services.AddSingleton(configuration);
            _services.AddSingleton<DiscordSocketClient>();
            _services.AddSingleton<InteractionHandler>();
            _services.AddSingleton(configuration);
            _services.AddScoped(typeof(ILoggerFacade<>), typeof(LoggerFacade<>));
        }

        public void SetupWorkers()
        {
            _services.AddSingleton<IPlaybackWorker, PlaybackWorker>();
        }

        public void SetupRepositories(IConfiguration config)
        {
            var sessionFactory = NHibernateSessionFactory;
            _services.AddSingleton(sessionFactory.OpenSession());

            _services.AddSingleton<IPlaylistRepository, PlaylistRepository>();
            _services.AddSingleton<IPlaylistEntriesRepository, PlaylistEntriesRepository>();

            //_services.AddScoped(typeof(PostgreBaseRepository<>));
            //MicroOrmConfig.SqlProvider = MicroOrm.Dapper.Repositories.SqlGenerator.SqlProvider.PostgreSQL;
            //var connectionString = config.GetSection("ConnectionStrings").GetSection("DefaultConnection").Get<string>();
            //IDbConnection db = new NpgsqlConnection(connectionString);
            //_services.AddSingleton(typeof(ISqlGenerator<>), typeof(SqlGenerator<>));
            //_services.AddSingleton<IScriptCordUnitOfWork>(new ScriptCordUnitOfWork(db));
        }

        // https://ivanderevianko.com/2015/04/vnext-use-postgresql-fluent-nhibernate-from-asp-net-5-dnx-on-ubuntu
        //private void TreatConfiguration()
        //{
        //    Action<string> updateExport = x =>
        //    {
        //        using (var file = new System.IO.FileStream(@"update.sql", System.IO.FileMode.Append, System.IO.FileAcce$))
        //    }
        //}

        public void SetupServices()
        {
            _services.AddScoped<IPlaylistService, PlaylistService>();
            _services.AddScoped<IPlaylistEntriesService, PlaylistEntriesService>();
        }

        public void SetupCommandModules()
        {
            _services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
        }

        public ServiceProvider Build()
            => _services.BuildServiceProvider();
    }
}
