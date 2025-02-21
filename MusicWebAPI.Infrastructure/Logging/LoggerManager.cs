using MusicWebAPI.Domain.Interfaces.Services.MusicWebAPI.Domain.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Infrastructure.Logging
{
    public class LoggerManager : ILoggerManager
    {
        private readonly ILogger _logger;

        public LoggerManager()
        {
            // Initialize the logger with basic configuration (console and file sinks as examples)
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/musicwebapi_log.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();
        }

        public void LogInfo(string message)
        {
            _logger.Information(message);
        }

        public void LogWarn(string message)
        {
            _logger.Warning(message);
        }

        public void LogError(string message)
        {
            _logger.Error(message);
        }

        public void LogDebug(string message)
        {
            _logger.Debug(message);
        }
    }
}
