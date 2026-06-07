using System.ComponentModel.DataAnnotations;

namespace Alojamientos.Business.DTOs.Habitaciones;

public record CrearHabitacionRequest(
    [Required] int AlojamientoId,
    [Required] [MaxLength(100)] string Nombre,
    [MaxLength(500)] string? Descripcion,
    int CapacidadAdultos = 2,
    int CapacidadNinos = 0,
    int NumBanos = 1,
    int NumDormitorios = 1,
    bool TieneCocina = false,
    bool TieneAireAcondicionado = false,
    decimal? SuperficieM2 = null,
    [Required] [Range(0, 10000)] decimal PrecioNoche = 0,
    string? Fotos = null
);

public record ActualizarHabitacionRequest(
    [Required] [MaxLength(100)] string Nombre,
    [MaxLength(500)] string? Descripcion,
    int CapacidadAdultos,
    int CapacidadNinos,
    int NumBanos,
    int NumDormitorios,
    bool TieneCocina,
    bool TieneAireAcondicionado,
    decimal? SuperficieM2,
    [Required] [Range(0, 10000)] decimal PrecioNoche,
    string? Fotos = null,
    [MaxLength(20)] string? Estado = "Activo"
);

public record HabitacionResponse(
    int HabitacionId,
    int AlojamientoId,
    string Nombre,
    string? Descripcion,
    int CapacidadAdultos,
    int CapacidadNinos,
    int NumBanos,
    int NumDormitorios,
    bool TieneCocina,
    bool TieneAireAcondicionado,
    decimal? SuperficieM2,
    decimal PrecioNoche,
    string Estado = "Activo",
    string? Fotos = null
);
