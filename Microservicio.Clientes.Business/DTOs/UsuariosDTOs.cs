using System.ComponentModel.DataAnnotations;

namespace Microservicio.Clientes.Business.DTOs.Usuarios;

// ── Request DTOs ─────────────────────────────────────
public record RegistrarClienteRequest(
    [Required] [EmailAddress] [MaxLength(200)] string Email,
    [Required] [MinLength(8)] [MaxLength(500)] string Password,
    [Required] [MaxLength(200)] string NombreCompleto,
    [Required] [RegularExpression(@"^\d+$", ErrorMessage = "La cédula debe contener solo números")] [StringLength(10, MinimumLength = 10)] string Cedula,
    [Required] [RegularExpression(@"^\d+$", ErrorMessage = "El teléfono debe contener solo números")] [MaxLength(20)] string Telefono,
    [Required] [MaxLength(300)] string Domicilio
);

public record ActualizarClienteRequest(
    [Required] [MaxLength(200)] string NombreCompleto,
    [Required] [RegularExpression(@"^\d+$")] [MaxLength(20)] string Telefono,
    [Required] [MaxLength(300)] string Domicilio,
    string? FotoUrl
);

public record CambiarEstadoRequest(
    [Required] bool Activo
);

public record LoginRequest(
    [Required] [EmailAddress] string Email,
    [Required] string Password
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

public record ClienteResponse(
    int ClienteId,
    int? UsuarioId,
    string Cedula,
    string? FotoUrl,
    string? Telefono,
    string? Domicilio,
    string Email,
    int TotalReservas,
    DateTime FechaCreacion,
    UsuarioResponse? Usuario
);

public record LoginResponse(
    string Token,
    UsuarioResponse Usuario
);
