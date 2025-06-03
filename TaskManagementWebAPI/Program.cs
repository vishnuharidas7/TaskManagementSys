using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using TaskManagementWebAPI.Data;
using TaskManagementWebAPI.Middlewares;
using TaskManagementWebAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
      options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITaskManagementRepository, TaskManagementRepository>();
builder.Services.AddHttpClient<IUserAuthRepository, UserAuthRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



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
