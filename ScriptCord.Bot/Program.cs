using Autofac;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ScriptCord.Bot.Commands;

namespace ScriptCord.Bot
{ 
    class Bot
    {
        public static void Main(string[] args)
            => new Bot().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            using (var container = SetupAutofac())
            {
                var scope = container.BeginLifetimeScope();

                var client = scope.Resolve<DiscordSocketClient>();
                client.Log += LogAsync;

                // TODO: This below could go into the constructor perhaps once the logging is moved to a class
                scope.Resolve<CommandService>().Log += LogAsync;

                // Do not accidentally upload an API token ;) 
                await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("token"));
                await Task.Delay(Timeout.Infinite);
            }
        }

        public IContainer SetupAutofac()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<DiscordSocketClient>().As<DiscordSocketClient>();
            builder.RegisterType<CommandService>().As<CommandService>();
            builder.RegisterType<HttpClient>().As<HttpClient>();

            builder.RegisterType<TestingModule>().As<TestingModule>();
            builder.RegisterType<CommandHandlingService>().As<CommandHandlingService>();
            return builder.Build();
        }

        private Task LogAsync(LogMessage log)
        {
            // TODO: normal logger for this
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}