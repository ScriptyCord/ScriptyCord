using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using ScriptCord.Bot.Commands;

namespace ScriptCord.Bot
{
    public class Program
    {
        static void Main(string[] args)
            => new Program().RunAsync()
                .GetAwaiter()
                .GetResult();

        private IocSetup _ioc;

        private Program()
        {
            var config = SetupConfiguration();
            _ioc = new IocSetup(config);
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

        private async Task RunAsync()
        {
            IServiceProvider services = _ioc.Build();
            var config = services.GetRequiredService<IConfiguration>();
            var client = services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;

            await services.GetRequiredService<InteractionHandler>()
                .InitializeAsync();

            var token = config.GetSection("discord").GetSection("token").Get<string>();
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private async Task LogAsync(LogMessage message)
            => Console.WriteLine(message.ToString());

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
