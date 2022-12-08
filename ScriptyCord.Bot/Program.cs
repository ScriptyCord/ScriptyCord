using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using ScriptCord.Bot.Commands;
using ScriptCord.Bot.Workers.Playback;

namespace ScriptCord.Bot
{
    public class Program
    {
        public static string Version = "dev-branch";

        static void Main(string[] args)
            => new Program()
                .RunAsync()
                .GetAwaiter()
                .GetResult();

        private IocSetup _ioc;
        private ILoggerFacade<Program> _logger;

        private Program()
        {
            var config = SetupConfiguration();
            SetupLogging();
            _ioc = new IocSetup(config);
            _ioc.SetupRepositories(config);
            _ioc.SetupServices();
            _ioc.SetupWorkers();
            _ioc.SetupCommandModules();
        }

        private IConfiguration SetupConfiguration()
        {
            string? env = Environment.GetEnvironmentVariable("ENVIRONMENT_TYPE");
            string inject = env != null ? $".{env}" : string.Empty;
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile($"appsettings{inject}.json")
                .Build();
            return config;
        }

        private void SetupLogging()
        {
            var config = new NLog.Config.LoggingConfiguration();

            var logconsole = new NLog.Targets.ColoredConsoleTarget("logconsole");
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);

            NLog.LogManager.Configuration = config;
        }

        private async Task RunAsync()
        {
            IServiceProvider services = _ioc.Build();
            var client = services.GetRequiredService<DiscordSocketClient>();
            var config = services.GetRequiredService<IConfiguration>();
            var token = config.GetSection("discord").GetSection("token").Get<string>();

            _logger = services.GetRequiredService<ILoggerFacade<Program>>();
            client.Log += LogAsync;

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            while (client.ConnectionState != ConnectionState.Connected)
                Thread.Sleep(1000);

            await services.GetRequiredService<InteractionHandler>()
                .InitializeAsync();

            Console.CancelKeyPress += delegate
            {
                _logger.Log(LogLevel.Info, "Gracefully shutting down the bot");
                services.GetRequiredService<DiscordSocketClient>().Dispose();
                services.GetRequiredService<IPlaybackWorker>().Stop();
                System.Environment.Exit(0);
            };

            _logger.SetupDiscordLogging(config, client, "general");
            await Task.Delay(Timeout.Infinite);
        }

        private async Task LogAsync(LogMessage message)
            => await _logger.LogAsync(message);

        public static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}
