using System.ComponentModel.DataAnnotations;

namespace Facturacion.Business.DTOs;

public record CrearFacturaRequest
{
    [Required]
    public int ReservaId { get; init; }

    /// <summary>ID interno (entero) del método de pago.</summary>
    public int? MetodoPagoId { get; init; }

    /// <summary>
    /// ID externo (UUID string) enviado por Booking.
    /// Si se envía, se resuelve automáticamente al MetodoPagoId interno.
    /// </summary>
    public string? MetodoPagoExternalId { get; init; }

    [Required]
    [Range(0.01, 1000000, ErrorMessage = "El monto debe ser mayor a 0.")]
    public decimal Monto { get; init; }

    public DateTime? FechaPago { get; init; }

    [Required]
    [MinLength(1, ErrorMessage = "Debe haber al menos un detalle en la factura.")]
    public List<CrearDetalleFacturaRequest> Detalles { get; init; } = new();
}
