using System.ComponentModel.DataAnnotations;

namespace Alojamientos.Business.DTOs.Fotos;

public record AgregarFotoRequest(
    [Required] int AlojamientoId,
    [Required] [MaxLength(500)] string Url,
    int Orden = 0,
    [MaxLength(200)] string? Descripcion = null
);

public record FotoResponse(
    int FotoId,
    int AlojamientoId,
    string Url,
    int Orden,
    string? Descripcion
);
