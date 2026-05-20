namespace Facturacion.Business.DTOs;

public record DetalleFacturaResponse
{
    public int DetalleFacturaId { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public int Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
    public decimal SubTotal => Cantidad * PrecioUnitario;
}
