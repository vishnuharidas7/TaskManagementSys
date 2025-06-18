namespace TaskManagementWebAPI.Infrastructure.Services.EmailService
{
    public class OverdueTaskEmailWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public OverdueTaskEmailWorker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();

                // Resolve your scoped services here inside the scope
                var dispatcher = scope.ServiceProvider.GetRequiredService<TaskEmailDispatcher>();

                await dispatcher.DispatchEmailsAsync();

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                //await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
        }
    }
}

     
