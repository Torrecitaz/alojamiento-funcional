namespace Microservicio.Clientes.Business.DTOs.Reservas;

// ── Request DTOs ─────────────────────────────────────
public record CrearReservaRequest(
    int ClienteId,
    int AlojamientoId,
    DateOnly FechaCheckIn,
    DateOnly FechaCheckOut,
    int NumAdultos,
    int NumNinos,
    bool LlevaMascotas,
    List<DetalleHabitacionRequest> Habitaciones
);

public record DetalleHabitacionRequest(
    int HabitacionId,
    decimal PrecioPorNoche
);

// ── Response DTOs ────────────────────────────────────
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
    List<DetalleHabitacionResponse>? Detalles,
    string? DescuentoCodigo
);

public record DetalleHabitacionResponse(
    int DetalleId,
    int HabitacionId,
    decimal PrecioPorNoche,
    int NumNoches,
    decimal SubTotalHabitacion
);
