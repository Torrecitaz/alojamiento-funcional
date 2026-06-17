using System;

namespace Shared.Kernel.Events;

public record ReservaCancelledEvent
{
    public int ReservaId { get; init; }
    public string CodigoReserva { get; init; } = string.Empty;
    public int ClienteId { get; init; }
    public int AlojamientoId { get; init; }
}
