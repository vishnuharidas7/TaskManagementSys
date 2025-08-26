using AuthenticationAPI.ConfigurationLayer;
using AuthenticationAPI.InfrastructureLayer.Data;
using AuthenticationAPI.InfrastructureLayer.Helpers;
using AuthenticationAPI.Repositories;
using AuthenticationAPI.Services;
using log4net;
using log4net.Config;
using LoggingLibrary;
using LoggingLibrary.Config;
using LoggingLibrary.Implementations;
using LoggingLibrary.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Configuration;
using System.Reflection;
using System.Text;


var congifuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.




builder.Services.AddControllers();


builder.Configuration.AddEnvironmentVariables();

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
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
}
);

// Bind and register JwtSettings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Register DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); 

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//      options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
//        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// Register custom services
builder.Services.AddScoped<IAuthService, AuthService>();

//Register JWT helper
builder.Services.AddScoped<IJwtHelper, JwtHelper>();

//Register Repository
builder.Services.AddScoped<IAuthRepository, AuthRepository>();

//Register Logg
builder.Services.AddSingleton(typeof(SerilogLogger<>));
builder.Services.AddSingleton(typeof(Log4NetLogger<>));

// Factory that chooses which one to use
builder.Services.AddSingleton(typeof(IAppLogger<>), typeof(AppLoggerFactory<>));

// Add JWT authentication
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

//NOSONAR
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
        //.WithOrigins("http://localhost:4200")
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

builder.Services.AddAuthorization();


var loggigProvider =builder.Configuration["Logging:LoggingProvider"];
if (loggigProvider == "Serilog")
{
    // Configure Serilog BEFORE building the app
    LoggerConfigurator.ConfigureLogging();
    // Tell the host to use Serilog for all logging
    builder.Host.UseSerilog();
}
if(loggigProvider == "Log4Net")
{
    // Load log4net config
    var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
    var log4netConfigPath = Path.Combine(AppContext.BaseDirectory, "Config", "log4net.config");
    XmlConfigurator.Configure(logRepository, new FileInfo(log4netConfigPath));
    Console.WriteLine($"Log4Net config exists? {File.Exists(log4netConfigPath)}");
}


var app = builder.Build();

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

await app.RunAsync();
