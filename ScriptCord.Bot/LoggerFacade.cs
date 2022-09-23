using CSharpFunctionalExtensions;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCord.Bot
{
    public interface ILoggerFacade<T>
    {
        void SetupDiscordLogging(IConfiguration configuration, DiscordSocketClient client, string targetLoggingChannelType);
        Task LogAsync(LogMessage message);
        void Log(LogLevel level, string log);
        public void LogInfo(string log);
        public void LogError(Result result);
        public void LogException(Exception exception);
        public void LogFatalException(Exception exception);
        public void LogDebug(string message);
    }

    public class LoggerFacade<T> : ILoggerFacade<T>
    {
        private readonly NLog.Logger _logger; 

        private Dictionary<LogSeverity, Action<string>> _discordSeverityLogProxy;

        private SocketTextChannel _textChannel = null;

        public LoggerFacade()
        {
            _logger = NLog.LogManager.GetLogger(typeof(T).Name);
            _discordSeverityLogProxy = new Dictionary<LogSeverity, Action<string>>()
            {
                { LogSeverity.Debug, _logger.Debug },
                { LogSeverity.Info, _logger.Info },
                { LogSeverity.Verbose, _logger.Info },
                { LogSeverity.Warning, _logger.Warn },
                { LogSeverity.Critical, _logger.Warn },
                { LogSeverity.Error, _logger.Error }
            };
        }

        public LoggerFacade(string loggerName)
        {
            _logger = NLog.LogManager.GetLogger(loggerName);
            _discordSeverityLogProxy = new Dictionary<LogSeverity, Action<string>>()
            {
                { LogSeverity.Debug, _logger.Debug },
                { LogSeverity.Info, _logger.Info },
                { LogSeverity.Verbose, _logger.Info },
                { LogSeverity.Warning, _logger.Warn },
                { LogSeverity.Critical, _logger.Warn },
                { LogSeverity.Error, _logger.Error }
            };
        }

        public void SetupDiscordLogging(IConfiguration configuration, DiscordSocketClient client, string targetLoggingChannelType)
        {
            if (configuration.GetSection("discord").GetValue<bool>("logToChannels"))
            {
                ulong guildId = configuration.GetSection("discord").GetSection("loggingChannels").GetValue<ulong>("guildId");
                ulong channelId = configuration.GetSection("discord").GetSection("loggingChannels").GetValue<ulong>($"{targetLoggingChannelType}Id");
                _textChannel = client.Guilds.First(x => x.Id == guildId).TextChannels.First(x => x.Id == channelId);
            }
        }

        public Task LogAsync(LogMessage log)
        {
            _discordSeverityLogProxy[log.Severity](log.Message);
            _textChannel?.SendMessageAsync($"`[{DateTime.UtcNow}][{typeof(T).Name}] {log.Message}`");
            return Task.CompletedTask;
        }

        public void Log(LogLevel level, string log)
        {
            _logger.Log(level, log);
            _textChannel?.SendMessageAsync($"`[{DateTime.UtcNow}][{typeof(T).Name}] {log}`");
        }

        public void LogInfo(string log)
        {
            _logger.Log(NLog.LogLevel.Info, log);
            _textChannel?.SendMessageAsync($"`[{DateTime.UtcNow}][{typeof(T).Name}] {log}`");
        }

        public void LogError(Result result)
        {
            _logger.Log(NLog.LogLevel.Error, result.Error);
            _textChannel?.SendMessageAsync($"`[{DateTime.UtcNow}][{typeof(T).Name}] {result.Error}`");
        }

        public void LogException(Exception exception)
        {
            _logger.Log(NLog.LogLevel.Error, exception);
            _textChannel?.SendMessageAsync($"`[{DateTime.UtcNow}][{typeof(T).Name}] {exception.Message}`");
        }

        public void LogFatalException(Exception exception)
        {
            _logger.Log(NLog.LogLevel.Fatal, exception);
            _textChannel?.SendMessageAsync($"`[{DateTime.UtcNow}][{typeof(T).Name}] {exception.Message}`");
        }

        public void LogDebug(string message)
        {
            _logger.Log(NLog.LogLevel.Debug, message);
            _textChannel?.SendMessageAsync($"`[{DateTime.UtcNow}][{typeof(T).Name}] {message}`");
        }
    }
}
