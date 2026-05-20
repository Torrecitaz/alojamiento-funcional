using System.ComponentModel.DataAnnotations;

namespace Reservas.Business.DTOs;

public record DetalleHabitacionRequest(
    [Required] int HabitacionId,
    [Required] [Range(1, 10000)] decimal PrecioPorNoche,
    [Required] [Range(1, 365)] int NumNoches
);
