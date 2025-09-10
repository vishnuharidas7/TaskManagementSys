
using log4net;
using log4net.Config;
using LoggingLibrary;
using LoggingLibrary.Config;
using LoggingLibrary.Implementations;
using LoggingLibrary.Interfaces;
using Scheduler.Configurations;
using Scheduler.Service;
using Scheduler.Services.EmailServices;
using Serilog;
using System.Reflection;




var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();
var loggingProvider = configuration["Logging:LoggingProvider"];
if (loggingProvider == "Serilog")
{
    LoggerConfigurator.ConfigureLogging(); // Custom Serilog configuration
}
else if (loggingProvider == "Log4Net")
{
    var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
    var log4netConfigPath = Path.Combine(AppContext.BaseDirectory, "Config", "log4net.config");
    XmlConfigurator.Configure(logRepository, new FileInfo(log4netConfigPath));
}

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
     .UseSerilog()
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;
        services.AddHttpClient();
     

        services.Configure<TaskCompletionReminderWorkerSettings>(
            configuration.GetSection("OverdueTaskEmailWorkerSettings"));
        services.Configure<TaskStatusUpdateWorkerSettings>(
          configuration.GetSection("TaskStatusUpdateServiceWorkerSettings"));

        services.AddSingleton(typeof(SerilogLogger<>));
        services.AddSingleton(typeof(Log4NetLogger<>));
        services.AddSingleton(typeof(IAppLogger<>), typeof(AppLoggerFactory<>));

        services.AddHostedService<TaskStatusUpdateWorker>();
        services.AddHostedService<TaskCompletionReminderWorker>();

    })
    .Build();

await host.RunAsync();
