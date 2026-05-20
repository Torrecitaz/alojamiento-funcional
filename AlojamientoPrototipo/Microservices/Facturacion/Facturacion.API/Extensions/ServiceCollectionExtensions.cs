using Microsoft.Extensions.DependencyInjection;
using Facturacion.DataAccess.Repositories;
using Facturacion.DataAccess.Repositories.Interfaces;
using Facturacion.DataManagement.Interfaces;
using Facturacion.DataManagement.Services;
using Facturacion.Business.Interfaces;
using Facturacion.Business.Services;

namespace Facturacion.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // ── DataAccess ──────────────────────────────────
        services.AddScoped<IFacturasRepository, FacturasRepository>();
        services.AddScoped<IDetalleFacturasRepository, DetalleFacturasRepository>();
        services.AddScoped<IMetodosPagoRepository, MetodosPagoRepository>();
        services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();

        // ── DataManagement ──────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IFacturasDataService, FacturasDataService>();
        services.AddScoped<IMetodosPagoDataService, MetodosPagoDataService>();
        services.AddScoped<IAuditoriaDataService, AuditoriaDataService>();

        // ── Business ────────────────────────────────────
        services.AddScoped<IFacturasService, FacturasService>();
        services.AddScoped<IMetodosPagoService, MetodosPagoService>();

        return services;
    }
}
