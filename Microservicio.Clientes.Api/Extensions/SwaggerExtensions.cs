using Microsoft.OpenApi.Models;

namespace Microservicio.Clientes.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddBookingSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "Booking API - Mateo Torres",
                Version     = "v1",
                Description = "Microservicio de gestión de reservas y clientes — Desarrollado por Mateo Torres",
                Contact     = null
            });

            // ── Botón "Authorize 🔒" para JWT en Swagger UI ──────────────
            var securityScheme = new OpenApiSecurityScheme
            {
                Name        = "Authorization",
                Description = "Ingrese: Bearer {token}",
                In          = ParameterLocation.Header,
                Type        = SecuritySchemeType.Http,
                Scheme      = "bearer",
                BearerFormat = "JWT",
                Reference   = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            };

            options.AddSecurityDefinition("Bearer", securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() }
            });
        });

        return services;
    }

    public static WebApplication UseBookingSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Booking API v1");
            options.RoutePrefix = "swagger"; // Ahora Swagger estará en /swagger
        });
        return app;
    }
}
