using LoggingLibrary.Implementations;
using LoggingLibrary.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggingLibrary
{
    public class AppLoggerFactory<T>:IAppLogger<T>
    {
        private readonly IAppLogger<T> _logger;

        public AppLoggerFactory(IServiceProvider serviceProvider, IConfiguration config)
        {
            var provider = config["Logging:LoggingProvider"];

            _logger = provider switch
            {
                "Serilog" => serviceProvider.GetRequiredService<SerilogLogger<T>>(),
                "Log4Net" => serviceProvider.GetRequiredService<Log4NetLogger<T>>(),
                _ => throw new InvalidOperationException($"Unknown logging provider: {provider}")
            };
        }

        public void LoggInformation(string message, params object[] args) => _logger.LoggInformation(message, args);
        public void LoggWarning(string message, params object[] args) => _logger.LoggWarning(message, args);
        public void LoggError(Exception exception, string message, params object[] args) =>
            _logger.LoggError(exception, message, args);
    }
}

