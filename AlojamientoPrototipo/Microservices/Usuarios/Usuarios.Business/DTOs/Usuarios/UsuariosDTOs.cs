using System.ComponentModel.DataAnnotations;

namespace Usuarios.Business.DTOs.Usuarios;

// ── Request DTOs ─────────────────────────────────────
public record CrearUsuarioRequest(
    [Required] [EmailAddress] [MaxLength(200)] string Email,
    [Required] [MinLength(8)] [MaxLength(500)] string Password,
    [Required] [MaxLength(200)] string NombreCompleto,
    [MaxLength(10)] string Rol = "Cliente"
);

public record ActualizarUsuarioRequest(
    [Required] [MaxLength(200)] string NombreCompleto,
    [EmailAddress] [MaxLength(200)] string? Email = null
);

public record UsuarioFiltroRequest(
    string? Search = null,
    int Page = 1,
    int PageSize = 10
);

// ── Response DTOs ────────────────────────────────────
public record UsuarioResponse(
    int UsuarioId,
    string Rol,
    string Email,
    string NombreCompleto,
    bool Estado,
    DateTime FechaCreacion
);
