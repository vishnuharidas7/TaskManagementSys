using Microsoft.EntityFrameworkCore;
using TaskManagementWebAPI.ConfigurationLayer;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Infrastructure.Persistence;
using TaskManagementWebAPI.Infrastructure.Repositories; 
using TaskManagementWebAPI.Infrastructure.Services.FileUpload;
using LoggingLibrary.Interfaces;
using LoggingLibrary.Implementations;
using LoggingLibrary;
using System.Data;
using FileParser.Factory;
using TaskManagementWebAPI.Application.Interfaces;
using TaskManagementWebAPI.Application.Services;
using TaskManagementWebAPI.Application.Services.FileUpload;
using TaskManagementWebAPI.Application.PasswordService;
using TaskManagementWebAPI.Application.Services.TaskStatusUpdateService;
using TaskManagementWebAPI.Application.Services.EmailService;

namespace TaskManagementWebAPI.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            //Register DbContext 

            var connectionString = configuration.GetConnectionString("DefaultConnection"); 
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
 

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITaskManagementRepository, TaskManagementRepository>();
            services.AddHttpClient<IUserAuthRepository, UserAuthRepository>();
            services.AddScoped<ITaskApplicationService, TaskApplicationService>();
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
            services.AddScoped<ITaskStatusUpdateServiceRepository, TaskStatusUpdateServiceRepository>();
            services.AddScoped<ITaskDueStatusUpdateService, TaskStatusUpdateApplicationService>();
            services.AddScoped<ITaskDueStatusUpdateInternalService, TaskDueStatusUpdateService>();
            services.AddScoped<ITaskUploadDapperRepository, TaskUploadDapperRepository>();

            // Email Service
            services.AddScoped<ITaskNotificationService, TaskEmailNotification>();
            services.AddScoped<IUserNotificationService, UserEmailNotification>();
            services.AddScoped<ITaskStatusContentBuilder, TaskCreatedContentBuilder>();
            services.AddScoped<ITaskStatusContentBuilder, TaskOnDueEmailContentBuilder>();
            services.AddScoped<ITaskStatusContentBuilder, TaskCompletedContentBuilder>();
            services.AddScoped<ITaskStatusContentBuilder, TaskOverDueEmailContentBuilder>();
            services.AddScoped<IEmailContentBuilder, TaskEmailContentBuilder>();
            services.AddScoped<IUserCreatedEmailContentBuilder, UserCreatedEmailContentBuilder>();
            services.AddSingleton(EmailServiceFactory.CreateEmailService(configuration));
            services.AddScoped<ITaskEmailDispatcher,TaskEmailDispatcher>();
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
