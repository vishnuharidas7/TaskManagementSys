using log4net;
using log4net.Config;
using LoggingLibrary;
using LoggingLibrary.Config;
using LoggingLibrary.Implementations;
using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Data;
using System.Reflection;
using System.Text;
using TaskManagementWebAPI.Domain.Interfaces;
using TaskManagementWebAPI.Infrastructure.Persistence;
using TaskManagementWebAPI.Infrastructure.Repositories;
using TaskManagementWebAPI.Infrastructure.Services.EmailService;
using TaskManagementWebAPI.Infrastructure.Services.FileUpload;
using TaskManagementWebAPI.Infrastructure.Services.PasswordService;
using TaskManagementWebAPI.Infrastructure.Services.TaskOndueUpdate;
using TaskManagementWebAPI.Infrastructure.Services.TaskStatusUpdateService;
using TaskManagementWebAPI.Middlewares;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

var congifuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
      options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));
 
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITaskManagementRepository, TaskManagementRepository>();
builder.Services.AddHttpClient<IUserAuthRepository, UserAuthRepository>();

//File Upload
builder.Services.AddScoped<ExcelTaskFileParser>();
builder.Services.AddScoped<CsvTaskFileParser>();
builder.Services.AddScoped<ITaskFileParserFactory, TaskFileParserFactory>();
builder.Services.AddScoped<IMaptoTasks, MaptoTasks>();
builder.Services.AddScoped<IDapperConnectionFactory, DapperConnectionFactory>();
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var factory = sp.GetRequiredService<IDapperConnectionFactory>();
    return factory.CreateConnection();
});
//Ends here

//Task Status update to due
builder.Services.AddScoped<ITaskStatusRepository, TaskStatusRepository>();
builder.Services.AddScoped<TaskStatusService>();
builder.Services.AddScoped<TaskApplicationService>();
builder.Services.AddHostedService<TaskStatusUpdateService>();
builder.Services.AddScoped<ITaskUploadDapperRepository, TaskUploadDapperRepository>();
//Ends here.....

//Email Service........
builder.Services.AddScoped<ITaskEmailRepository, InMemoryTaskRepository>();
builder.Services.AddScoped<IUserEmailRepository, InMemoryUserRepository>();

builder.Services.AddScoped<ITaskStatusContentBuilder, NewTaskContentBuilder>();
builder.Services.AddScoped<ITaskStatusContentBuilder, OnDueTaskContentBuilder>();
builder.Services.AddScoped<ITaskStatusContentBuilder, CompletedTaskContentBuilder>();
builder.Services.AddScoped<ITaskStatusContentBuilder, OverDueTaskContentBuilder>();


builder.Services.AddScoped<IEmailContentBuilder, TaskEmailContentBuilder>();

builder.Services.AddScoped<INewUserEmailContentBuilder, NewUserEmailContentBuilder>();

var config = builder.Configuration;
builder.Services.AddSingleton(EmailServiceFactory.CreateEmailService(config));
builder.Services.AddScoped<TaskEmailDispatcher>();

builder.Services.AddHostedService<OverdueTaskEmailWorker>();

builder.Services.AddScoped<GmailSmtpEmailService>();

builder.Services.AddScoped<IForgotPasswordHandler, ForgotPasswordHandler>();
// ENDS here.....

//Random Password generator
builder.Services.AddScoped<IRandomPasswordGenerator,RandomPasswordGenerator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme,
        securityScheme: new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "JWT Authorization header using the Bearer scheme",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference =new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    new string[] {}
                }
                });
}
);

//Register Logg
builder.Services.AddSingleton(typeof(SerilogLogger<>));
builder.Services.AddSingleton(typeof(Log4NetLogger<>));

// Factory that chooses which one to use
builder.Services.AddSingleton(typeof(IAppLogger<>), typeof(AppLoggerFactory<>));


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
        //.WithOrigins("http://localhost:4200")
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100MB
});

builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 104857600; // 100MB
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
        };
    });


var loggigProvider = builder.Configuration["Logging:LoggingProvider"];
if (loggigProvider == "Serilog")
{
    // Configure Serilog BEFORE building the app
    LoggerConfigurator.ConfigureLogging();
    // Tell the host to use Serilog for all logging
    builder.Host.UseSerilog();
}
if (loggigProvider == "Log4Net")
{
    // Load log4net config
    var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
    var log4netConfigPath = Path.Combine(AppContext.BaseDirectory, "Config", "log4net.config");
    XmlConfigurator.Configure(logRepository, new FileInfo(log4netConfigPath));
    Console.WriteLine($"Log4Net config exists? {File.Exists(log4netConfigPath)}");
}
var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
