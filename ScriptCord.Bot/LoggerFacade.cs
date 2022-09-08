using CSharpFunctionalExtensions;
using Discord;
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
        Task LogAsync(LogMessage message);
        void Log(LogLevel level, string log);
    }

    public class LoggerFacade<T> : ILoggerFacade<T>
    {
        private readonly NLog.Logger _logger; 

        private Dictionary<LogSeverity, Action<string>> _discordSeverityLogProxy;

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

        public Task LogAsync(LogMessage log)
        {
            _discordSeverityLogProxy[log.Severity](log.Message);
            return Task.CompletedTask;
        }

        public void Log(LogLevel level, string log)
            => _logger.Log(level, log);

        public void LogInfo(string log)
            => _logger.Log(NLog.LogLevel.Info, log);

        public void LogError(Result result)
            => _logger.Log(NLog.LogLevel.Error, result.Error);

        public void LogException(Exception exception)
            => _logger.Log(NLog.LogLevel.Error, exception);

        public void LogFatalException(Exception exception)
            => _logger.Log(NLog.LogLevel.Fatal, exception);

        public void LogDebug(string message)
            => _logger.Log(NLog.LogLevel.Debug, message);
    }
}
