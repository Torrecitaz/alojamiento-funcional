namespace Shared.Kernel.Events;

public record FacturaPagadaEvent
{
    public int ReservaId { get; init; }
    public int FacturaId { get; init; }
    public decimal MontoPagado { get; init; }
    public DateTime FechaPago { get; init; }
}
