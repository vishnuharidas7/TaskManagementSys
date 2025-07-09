using Microsoft.EntityFrameworkCore;
using TaskManagementWebAPI.ConfigurationLayer;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Infrastructure.Persistence;
using TaskManagementWebAPI.Infrastructure.Repositories;
using TaskManagementWebAPI.Infrastructure.Services.EmailService;
using TaskManagementWebAPI.Infrastructure.Services.FileUpload;
using TaskManagementWebAPI.Infrastructure.Services.PasswordService;
//using TaskManagementWebAPI.Infrastructure.Services.TaskOndueUpdate;
using TaskManagementWebAPI.Infrastructure.Services.TaskStatusUpdateService;
using LoggingLibrary.Interfaces;
using LoggingLibrary.Implementations;
using LoggingLibrary;
using System.Data;
using FileParser.Factory;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Application.Services;

namespace TaskManagementWebAPI.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                  options.UseMySql(configuration.GetConnectionString("DefaultConnection"),
                  ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITaskManagementRepository, TaskManagementRepository>();
            services.AddHttpClient<IUserAuthRepository, UserAuthRepository>();
            services.AddScoped<ITaskApplicartionService, TaskApplicartionService>();
            services.AddScoped<IUserApplicationService, UserApplicationService>();

            // File Upload
            services.AddScoped<ExcelTaskFileParser>();
            services.AddScoped<CsvTaskFileParser>();
            services.AddScoped<ITaskFileParserFactory, TaskFileParserFactory>();
            services.AddScoped<IMaptoTasks, MaptoTasks>();
            services.AddScoped<IDapperConnectionFactory, DapperConnectionFactory>();
            services.AddScoped<IDbConnection>(sp =>
            {
                var factory = sp.GetRequiredService<IDapperConnectionFactory>();
                return factory.CreateConnection();
            });

            // Task status
            services.AddScoped<ITaskStatusRepository, TaskStatusRepository>();
            services.AddScoped<TaskStatusService>();
            services.AddScoped<TTaskApplicationServices, TaskApplicationService>();
          //  services.AddHostedService<TaskStatusUpdateService>();
            services.AddScoped<ITaskUploadDapperRepository, TaskUploadDapperRepository>();

            // Email Service
            services.AddScoped<ITaskEmailRepository, InMemoryTaskRepository>();
            services.AddScoped<IUserEmailRepository, InMemoryUserRepository>();
            services.AddScoped<ITaskStatusContentBuilder, NewTaskContentBuilder>();
            services.AddScoped<ITaskStatusContentBuilder, OnDueTaskContentBuilder>();
            services.AddScoped<ITaskStatusContentBuilder, CompletedTaskContentBuilder>();
            services.AddScoped<ITaskStatusContentBuilder, OverDueTaskContentBuilder>();
            services.AddScoped<IEmailContentBuilder, TaskEmailContentBuilder>();
            services.AddScoped<INewUserEmailContentBuilder, NewUserEmailContentBuilder>();
            services.AddSingleton(EmailServiceFactory.CreateEmailService(configuration));
            services.AddScoped<ITaskEmailDispatcher,TaskEmailDispatcher>();
          //  services.AddHostedService<OverdueTaskEmailWorker>();
            services.AddScoped<GmailSmtpEmailService>();
            services.AddScoped<IForgotPasswordHandler, ForgotPasswordHandler>();

            // Password
            services.AddScoped<IRandomPasswordGenerator, RandomPasswordGenerator>();

            // AppSettings
            services.Configure<TaskSettings>(configuration.GetSection("TaskSettings"));

            // Logging
            services.AddSingleton(typeof(SerilogLogger<>));
            services.AddSingleton(typeof(Log4NetLogger<>));
            services.AddSingleton(typeof(IAppLogger<>), typeof(AppLoggerFactory<>));

            return services;
        }
    }
}
