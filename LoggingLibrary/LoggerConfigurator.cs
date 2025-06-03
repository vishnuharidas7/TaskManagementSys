using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggingLibrary
{
    public static class LoggerConfigurator
    {
        public static void ConfigureLogging(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("D:\\TaskManagement-Collab\\Back-End\\TaskManagementSys\\LoggingLibrary\\Logs\\Auth\\AuthLog.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7, // optional
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("D:\\TaskManagement-Collab\\Back-End\\TaskManagementSys\\LoggingLibrary\\Logs\\TaskManagement\\TaskLog.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7, // optional
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();
        }

    }
}
