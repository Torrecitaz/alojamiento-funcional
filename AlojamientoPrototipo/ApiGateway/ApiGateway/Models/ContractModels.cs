using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Models;

public record CrearReservaRequest
{
    [Required]
    public int ClienteId { get; init; }
    
    [Required]
    public int AlojamientoId { get; init; }
    
    [Required]
    public DateOnly FechaCheckIn { get; init; }
    
    [Required]
    public DateOnly FechaCheckOut { get; init; }
    
    [Required]
    public int NumeroHuespedes { get; init; }
    
    [Required]
    public List<DetalleHabitacionRequest> Habitaciones { get; init; } = new();
}

public record DetalleHabitacionRequest
{
    [Required]
    public int HabitacionId { get; init; }
}

public record ReservaResponse
{
    public int ReservaId { get; init; }
    public string CodigoConfirmacion { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public decimal PrecioTotal { get; init; }
}
