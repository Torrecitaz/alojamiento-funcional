using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Facturacion.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Microservicio Facturación API",
                Version = "v1",
                Description = "API de gestión de facturación (Reto 2)"
            });
        });

        return services;
    }
}
