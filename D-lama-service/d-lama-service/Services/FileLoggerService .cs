using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace d_lama_service.Services
{
    public class FileLoggerService : ILoggerService
    {
        private readonly ILogger<FileLoggerService> _logger;
        private readonly string _logFilePath;

        public FileLoggerService(ILogger<FileLoggerService> logger)
        {
            _logger = logger;
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", GetLogFileName());
        }

        public void LogInformation(int userId, string message)
        {
            LogToFile(LogLevel.Information, userId.ToString(), message);
            _logger.LogInformation(message);
        }

        public void LogException(Exception ex)
        {
            string userId = "-";
            LogToFile(LogLevel.Error, userId, $"Exception: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}");
            _logger.LogError(ex, ex.Message);
        }

        private void LogToFile(LogLevel logLevel, string userId, string message)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(_logFilePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath));

                DateTime now = DateTime.Now;
                string timestamp = now.ToString("HH:mm:ss");
                string logEntry = $"{{\"timestamp\": \"{timestamp}\"," +
                    $"\"level\": \"{logLevel}\"," +
                    $"\"userId\": \"{userId}\"," +
                    $"\"message\": \"{message}\"}}";
                File.AppendAllText(_logFilePath, $"{logEntry}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while writing to log file");
            }
        }

        private string GetLogFileName()
        {
            string currentDate = DateTime.Now.ToString("yyyyMMdd");
            return $"log_{currentDate}.json";
        }
    }
}
