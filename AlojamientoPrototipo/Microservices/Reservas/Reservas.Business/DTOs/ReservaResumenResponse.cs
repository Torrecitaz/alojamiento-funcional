namespace Reservas.Business.DTOs;

/// <summary>
/// DTO liviano para listados. No incluye detalles de habitaciones.
/// </summary>
public record ReservaResumenResponse(
    int ReservaId,
    int ClienteId,
    int AlojamientoId,
    DateOnly FechaCheckIn,
    DateOnly FechaCheckOut,
    int NumHabitaciones,
    decimal Total,
    string Estado,
    string CodigoReserva,
    DateTime FechaCreacion
);
