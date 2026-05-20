using System.ComponentModel.DataAnnotations;

namespace Alojamientos.Business.DTOs.Alojamientos;

public record CrearAlojamientoRequest(
    [Required] int SocioId, // El id del usuario propietario
    [Required] int TipoAlojamientoId,
    [Required] [MaxLength(200)] string Nombre,
    [MaxLength(100)] string? Ciudad,
    [Required] [MaxLength(300)] string Direccion,
    string? Descripcion,
    bool AdmiteMascotas = false,
    bool TienePiscina = false,
    bool TieneParqueadero = false
);

public record ActualizarAlojamientoRequest(
    [Required] [MaxLength(200)] string Nombre,
    [MaxLength(100)] string? Ciudad,
    [Required] [MaxLength(300)] string Direccion,
    string? Descripcion,
    [Required] int TipoAlojamientoId,
    bool AdmiteMascotas,
    bool TienePiscina,
    bool TieneParqueadero,
    int? Estrellas
);

public record AlojamientoResponse(
    int AlojamientoId,
    int SocioId,
    int TipoAlojamientoId,
    string TipoAlojamientoNombre,
    string Nombre,
    string? Ciudad,
    string Direccion,
    string? Descripcion,
    int? Estrellas,
    decimal CalificacionPromedio,
    int TotalResenas,
    bool AdmiteMascotas,
    bool TienePiscina,
    bool TieneParqueadero,
    string Estado,
    DateTime FechaCreacion
);
