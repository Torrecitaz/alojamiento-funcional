using Microsoft.Extensions.DependencyInjection;
using Usuarios.DataAccess.Repositories;
using Usuarios.DataAccess.Repositories.Interfaces;
using Usuarios.DataManagement.Interfaces;
using Usuarios.DataManagement.Services;
using Usuarios.Business.Interfaces;
using Usuarios.Business.Services;

namespace Usuarios.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // ── DataAccess ──────────────────────────────────
        services.AddScoped<IUsuariosRepository, UsuariosRepository>();
        services.AddScoped<IClientesRepository, ClientesRepository>();
        services.AddScoped<ILocalizacionesRepository, LocalizacionesRepository>();

        // ── DataManagement ──────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUsuariosDataService, UsuariosDataService>();
        services.AddScoped<IClientesDataService, ClientesDataService>();
        services.AddScoped<ILocalizacionesDataService, LocalizacionesDataService>();

        // ── Business ────────────────────────────────────
        services.AddScoped<IUsuariosService, UsuariosService>();
        services.AddScoped<IClientesService, ClientesService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILocalizacionesService, LocalizacionesService>();

        return services;
    }
}
