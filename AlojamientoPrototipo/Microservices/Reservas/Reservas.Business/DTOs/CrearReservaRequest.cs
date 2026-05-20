using System.ComponentModel.DataAnnotations;

namespace Reservas.Business.DTOs;

public record CrearReservaRequest(
    [Required] int ClienteId,
    [Required] int AlojamientoId,
    [Required] DateOnly FechaCheckIn,
    [Required] DateOnly FechaCheckOut,
    int NumAdultos = 1,
    int NumNinos = 0,
    bool LlevaMascotas = false,
    string? CodigoDescuento = null,
    [Required] List<DetalleHabitacionRequest> Habitaciones = null!
);
