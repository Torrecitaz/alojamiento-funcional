using System;

namespace Shared.Kernel.Events;

public record ReservaCreatedEvent
{
    public int ReservaId { get; init; }
    public int AlojamientoId { get; init; }
    public int ClienteId { get; init; }
    public DateOnly FechaCheckIn { get; init; }
    public DateOnly FechaCheckOut { get; init; }
    public decimal Total { get; init; }
    public string CodigoReserva { get; init; } = string.Empty;
}
