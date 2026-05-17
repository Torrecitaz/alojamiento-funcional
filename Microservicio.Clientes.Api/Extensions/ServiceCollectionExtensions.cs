using Microservicio.Cliente.DatAccess.Contexts;
using Microservicio.Cliente.DatAccess.Entities.Alojamientos;
using Microservicio.Clientes.Business.Interfaces;
using Microservicio.Clientes.Business.Services;
using Microservicio.Clientes.DataManagement.Interfaces;
using Microservicio.Clientes.DataManagement.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Clientes.Api.Extensions;

/// <summary>
/// Registra todos los servicios de las capas 1, 2 y 3 en el contenedor DI.
/// Centraliza la inyección de dependencias, manteniendo Program.cs limpio.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBookingServices(this IServiceCollection services, IConfiguration config)
    {
        // ── Capa 1: DbContexts (4 bases de datos Supabase) ──────────────
        services.AddDbContext<UsuariosDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("UsuariosDB"))
                   .UseLowerCaseNamingConvention());

        services.AddDbContext<AlojamientosDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("AlojamientosDB"))
                   .UseLowerCaseNamingConvention());

        services.AddDbContext<ReservasDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("ReservasDB"))
                   .UseLowerCaseNamingConvention());

        services.AddDbContext<FacturacionDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("FacturacionDB"))
                   .UseLowerCaseNamingConvention());

        // ── Capa 2: DataManagement (Repositorios) ────────────────────────
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IAlojamientoRepository, AlojamientoRepository>();
        services.AddScoped<IHabitacionRepository, HabitacionRepository>();
        services.AddScoped<ICalendarioRepository, CalendarioRepository>();
        services.AddScoped<IReservaRepository, ReservaRepository>();
        services.AddScoped<IFacturaRepository, FacturaRepository>();

        // Repositorio genérico para TiposAlojamiento
        services.AddScoped<IRepository<TipoAlojamientoEntity>>(sp =>
            new Repository<TipoAlojamientoEntity>(sp.GetRequiredService<AlojamientosDbContext>()));

        // ── Capa 3: Business (Servicios y Orquestación) ──────────────────
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IAlojamientoService, AlojamientoService>();
        services.AddScoped<IReservaService, ReservaService>();
        services.AddScoped<IFacturaService, FacturaService>();

        return services;
    }
}
