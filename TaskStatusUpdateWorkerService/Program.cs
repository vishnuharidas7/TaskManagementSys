
using LoggingLibrary;
using LoggingLibrary.Implementations;
using LoggingLibrary.Interfaces;
using Scheduler.Configurations;
using Scheduler.Service;
using Scheduler.Services.EmailServices;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient();
        var configuration = hostContext.Configuration;

        services.Configure<OverdueTaskEmailWorkerSettings>(
            configuration.GetSection("OverdueTaskEmailWorkerSettings"));
        services.Configure<TaskStatusUpdateServiceWorkerSettings>(
          configuration.GetSection("TaskStatusUpdateServiceWorkerSettings"));

        services.AddSingleton(typeof(SerilogLogger<>));
        services.AddSingleton(typeof(Log4NetLogger<>));
        services.AddSingleton(typeof(IAppLogger<>), typeof(AppLoggerFactory<>));

        services.AddHostedService<TaskStatusUpdateServiceWorkerService>();
        services.AddHostedService<OverdueTaskEmailWorkerService>();

    })
    .Build();

await host.RunAsync();
