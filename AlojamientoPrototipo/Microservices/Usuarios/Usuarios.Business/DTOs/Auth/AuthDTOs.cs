using System.ComponentModel.DataAnnotations;

namespace Usuarios.Business.DTOs.Auth;

public record LoginRequest(
    [Required] [EmailAddress] string Email,
    [Required] string Password
);

public record LoginResponse(
    string Token,
    string Rol,
    string NombreCompleto
);
