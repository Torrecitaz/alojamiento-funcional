using Microsoft.Extensions.DependencyInjection;
using Alojamientos.DataAccess.Repositories;
using Alojamientos.DataAccess.Repositories.Interfaces;
using Alojamientos.DataManagement.Interfaces;
using Alojamientos.DataManagement.Services;
using Alojamientos.Business.Interfaces;
using Alojamientos.Business.Services;

namespace Alojamientos.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // ── DataAccess ──────────────────────────────────
        services.AddScoped<IAlojamientosRepository, AlojamientosRepository>();
        services.AddScoped<IHabitacionesRepository, HabitacionesRepository>();
        services.AddScoped<ITiposAlojamientoRepository, TiposAlojamientoRepository>();
        services.AddScoped<IAlojamientoFotosRepository, AlojamientoFotosRepository>();
        services.AddScoped<ICalendarioDisponibilidadRepository, CalendarioDisponibilidadRepository>();

        // ── DataManagement ──────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAlojamientosDataService, AlojamientosDataService>();
        services.AddScoped<IHabitacionesDataService, HabitacionesDataService>();
        services.AddScoped<IAlojamientoFotosDataService, AlojamientoFotosDataService>();
        services.AddScoped<ICalendarioDataService, CalendarioDataService>();

        // ── Business ────────────────────────────────────
        services.AddScoped<IAlojamientosService, AlojamientosService>();
        services.AddScoped<IHabitacionesService, HabitacionesService>();
        services.AddScoped<IFotosService, FotosService>();
        services.AddScoped<ICalendarioService, CalendarioService>();

        return services;
    }
}
