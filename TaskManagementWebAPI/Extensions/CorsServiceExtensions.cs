
namespace TaskManagementWebAPI.Extensions
{
    public static class CorsServiceExtensions
    {
        public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
        {
            //NOSONAR
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>//NOSONAR
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}
