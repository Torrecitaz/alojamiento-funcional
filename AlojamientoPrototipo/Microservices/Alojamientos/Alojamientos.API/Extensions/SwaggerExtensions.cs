using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Alojamientos.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Microservicio Alojamientos API",
                Version = "v1",
                Description = "API de gestión de alojamientos, habitaciones y fotos (Reto 2)"
            });
        });

        return services;
    }
}
