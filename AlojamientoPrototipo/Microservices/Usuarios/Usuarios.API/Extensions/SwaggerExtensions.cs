using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Usuarios.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Microservicio Usuarios API",
                Version = "v1",
                Description = "API de gestión de usuarios y clientes para AlojamientoPrototipo (Reto 2)"
            });

            // Aquí se agregará la configuración JWT en el futuro
        });

        return services;
    }
}
