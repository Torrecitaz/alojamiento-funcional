using System;

namespace Shared.Kernel.Events;

public record HabitacionDisponibilidadChangedEvent
{
    public int HabitacionId { get; init; }
    public DateOnly Fecha { get; init; }
    public string Estado { get; init; } = string.Empty; // Disponible, Ocupado, Bloqueado
}
