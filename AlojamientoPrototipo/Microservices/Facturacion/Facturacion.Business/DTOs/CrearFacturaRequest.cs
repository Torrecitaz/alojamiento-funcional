using System.ComponentModel.DataAnnotations;

namespace Facturacion.Business.DTOs;

public record CrearFacturaRequest
{
    [Required]
    public int ReservaId { get; init; }

    public int? MetodoPagoId { get; init; }

    [Required]
    [Range(0.01, 1000000, ErrorMessage = "El monto debe ser mayor a 0.")]
    public decimal Monto { get; init; }

    public DateTime? FechaPago { get; init; }

    [Required]
    [MinLength(1, ErrorMessage = "Debe haber al menos un detalle en la factura.")]
    public List<CrearDetalleFacturaRequest> Detalles { get; init; } = new();
}
