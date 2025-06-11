using Serilog;

namespace LoggingLibrary.Config
{
    public static class LoggerConfigurator
    {
        public static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
               // .ReadFrom.Configuration(configuration)
                //.Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("D:\\TaskManagement-Collab-Log\\Logg\\SeriLog-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                //.WriteTo.File("D:\\TaskManagement-Collab-Log\\TaskManagement\\TaskLogS-.txt",
                //rollingInterval: RollingInterval.Day,
                //retainedFileCountLimit: 7,
                //shared: true,
                //outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                //)
                .CreateLogger();
        }

    }
}
