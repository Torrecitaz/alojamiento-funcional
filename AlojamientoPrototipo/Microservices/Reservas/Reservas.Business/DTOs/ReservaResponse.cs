namespace Reservas.Business.DTOs;

public record ReservaResponse(
    int ReservaId,
    int ClienteId,
    int AlojamientoId,
    DateOnly FechaCheckIn,
    DateOnly FechaCheckOut,
    int NumAdultos,
    int NumNinos,
    bool LlevaMascotas,
    int NumHabitaciones,
    decimal SubTotal,
    decimal Total,
    string Estado,
    string CodigoReserva,
    DateTime FechaCreacion,
    List<DetalleHabitacionResponse> DetallesHabitacion,
    string? CodigoDescuentoAplicado,
    decimal? PorcentajeDescuento
);
