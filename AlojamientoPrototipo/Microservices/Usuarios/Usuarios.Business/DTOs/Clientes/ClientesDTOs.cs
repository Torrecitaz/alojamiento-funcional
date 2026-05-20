using System.ComponentModel.DataAnnotations;
using Usuarios.Business.DTOs.Usuarios;

namespace Usuarios.Business.DTOs.Clientes;

// ── Request DTOs ─────────────────────────────────────
public record RegistrarClienteRequest(
    [Required] [EmailAddress] [MaxLength(200)] string Email,
    [Required] [MinLength(8)] [MaxLength(500)] string Password,
    [Required] [MaxLength(200)] string NombreCompleto,
    [Required] [RegularExpression(@"^\d+$", ErrorMessage = "La cédula debe contener solo números")]
    [StringLength(10, MinimumLength = 10)] string Cedula,
    [Required] [RegularExpression(@"^\d+$", ErrorMessage = "El teléfono debe contener solo números")]
    [MaxLength(20)] string Telefono,
    [Required] [MaxLength(300)] string Domicilio
);

public record ActualizarClienteRequest(
    [Required] [MaxLength(200)] string NombreCompleto,
    [Required] [RegularExpression(@"^\d+$")] [MaxLength(20)] string Telefono,
    [Required] [MaxLength(300)] string Domicilio,
    string? FotoUrl = null
);

public record CambiarEstadoRequest(
    [Required] bool Activo
);

public record ClienteFiltroRequest(
    string? Nombre = null,
    int Page = 1,
    int PageSize = 10
);

// ── Response DTOs ────────────────────────────────────
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
