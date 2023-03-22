using NLog;
using P2PWallet.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Services
{
    public class LoggerManager : ILoggerManager
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger(); 
        public void LogDebug(string message)
        {
            _logger.Debug(message);
        }

        public void LogError(string message)
        {
           _logger.Error(message);
        }

        public void LogInformation(string message)
        {
           _logger.Info(message);
        }

        public void LogWarning(string message)
        {
            _logger?.Warn(message);
        }
    }
}
