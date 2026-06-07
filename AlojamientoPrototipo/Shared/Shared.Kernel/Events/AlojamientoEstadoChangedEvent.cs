using System;

namespace Shared.Kernel.Events;

public record AlojamientoEstadoChangedEvent
{
    public int AlojamientoId { get; init; }
    public string Estado { get; init; } = string.Empty; // Activo, Inactivo
}
