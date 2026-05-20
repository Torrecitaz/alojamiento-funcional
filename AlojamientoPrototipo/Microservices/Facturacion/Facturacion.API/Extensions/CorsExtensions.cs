using Microsoft.Extensions.DependencyInjection;

namespace Facturacion.API.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCustomCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader());
        });

        return services;
    }
}
