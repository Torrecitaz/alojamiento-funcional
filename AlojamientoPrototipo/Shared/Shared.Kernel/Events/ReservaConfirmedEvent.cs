using System;

namespace Shared.Kernel.Events;

public record ReservaConfirmedEvent
{
    public int ReservaId { get; init; }
    public string CodigoReserva { get; init; } = string.Empty;
    public int ClienteId { get; init; }
    public int AlojamientoId { get; init; }
    public DateOnly FechaCheckIn { get; init; }
    public DateOnly FechaCheckOut { get; init; }
}
