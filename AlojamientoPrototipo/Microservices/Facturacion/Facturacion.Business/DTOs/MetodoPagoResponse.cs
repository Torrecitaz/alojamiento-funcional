namespace Facturacion.Business.DTOs;

public record MetodoPagoResponse
{
    public int MetodoPagoId { get; init; }
    public string Tipo { get; init; } = string.Empty;
}
