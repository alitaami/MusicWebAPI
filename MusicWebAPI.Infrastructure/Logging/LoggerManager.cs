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
        public void LogInfo(string message)
        {
            Log.Information(message);
        }

        public void LogWarn(string message)
        {
            Log.Warning(message);
        }

        public void LogError(string message)
        {
            Log.Error(message);
        }

        public void LogDebug(string message)
        {
            Log.Debug(message);
        }
    }
}
