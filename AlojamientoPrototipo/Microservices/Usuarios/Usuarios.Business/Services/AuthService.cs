using Usuarios.Business.DTOs.Auth;
using Usuarios.Business.Interfaces;

namespace Usuarios.Business.Services;

/// <summary>
/// Stub: Servicio de autenticación. Retorna null por ahora.
/// Se implementará con JWT en fases posteriores.
/// </summary>
public class AuthService : IAuthService
{
    public Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        // TODO: Implementar validación de credenciales y generación de JWT
        return Task.FromResult<LoginResponse?>(null);
    }
}
