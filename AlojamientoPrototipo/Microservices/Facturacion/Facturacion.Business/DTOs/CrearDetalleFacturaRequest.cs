using System.ComponentModel.DataAnnotations;

namespace Facturacion.Business.DTOs;

public record CrearDetalleFacturaRequest
{
    [Required]
    [MaxLength(200)]
    public string Descripcion { get; init; } = string.Empty;

    [Required]
    [Range(1, 100)]
    public int Cantidad { get; init; }

    [Required]
    [Range(0.01, 1000000)]
    public decimal PrecioUnitario { get; init; }
}
