using log4net;
using log4net.Config;
using LoggingLibrary.Config;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Configuration;
using System.Reflection;
using TaskManagementWebAPI.Extensions;
using TaskManagementWebAPI.Infrastructure.Persistence;
using TaskManagementWebAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSwaggerDocumentation();
builder.Services.AddCorsPolicy();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);


builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104857600;
});

builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 104857600;
});

var loggigProvider = builder.Configuration["Logging:LoggingProvider"];
if (loggigProvider == "Serilog")
{
    LoggerConfigurator.ConfigureLogging();
    builder.Host.UseSerilog();
}
if (loggigProvider == "Log4Net")
{
    var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
    var log4netConfigPath = Path.Combine(AppContext.BaseDirectory, "Config", "log4net.config");
    XmlConfigurator.Configure(logRepository, new FileInfo(log4netConfigPath));
}

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

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

await app.RunAsync();

