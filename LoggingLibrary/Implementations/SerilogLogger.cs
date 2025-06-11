using LoggingLibrary.Interfaces;
using Microsoft.Extensions.Logging;

namespace LoggingLibrary.Implementations
{
    public class SerilogLogger<T>:IAppLogger<T>
    {
        private readonly ILogger<T> _logger;

        public SerilogLogger(ILogger<T> logger)
        {
            this._logger = logger;
        }
        public void LoggInformation(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }
        public void LoggWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }
        public void LoggError(Exception exception, string message, params object[] args)
        {
            _logger.LogError(exception,message, args);
        }
    }
}
