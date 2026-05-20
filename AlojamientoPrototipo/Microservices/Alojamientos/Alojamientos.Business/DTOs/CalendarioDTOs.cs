using System.ComponentModel.DataAnnotations;

namespace Alojamientos.Business.DTOs;

public record CalendarioResponse
{
    public int CalendarioId { get; init; }
    public int HabitacionId { get; init; }
    public DateOnly Fecha { get; init; }
    public string Estado { get; init; } = string.Empty;
}

public record BloquearFechasRequest
{
    [Required]
    public int HabitacionId { get; init; }
    
    [Required]
    public DateOnly FechaInicio { get; init; }
    
    [Required]
    public DateOnly FechaFin { get; init; }
}
