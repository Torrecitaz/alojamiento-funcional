using Usuarios.Business.DTOs.Auth;

namespace Usuarios.Business.Interfaces;

/// <summary>
/// Stub: Servicio de autenticación. Se implementará funcional en fases posteriores.
/// </summary>
public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}
