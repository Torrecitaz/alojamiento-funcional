namespace Facturacion.Business.DTOs;

public record FacturaResponse
{
    public int FacturaId { get; init; }
    public int ReservaId { get; init; }
    public int? MetodoPagoId { get; init; }
    public string? MetodoPagoTipo { get; init; }
    public decimal Monto { get; init; }
    public string Estado { get; init; } = string.Empty;
    public DateTime? FechaPago { get; init; }
    public DateTime FechaCreacion { get; init; }
    public List<DetalleFacturaResponse> Detalles { get; init; } = new();
}
