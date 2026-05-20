namespace Facturacion.DataManagement.Models;

public class FacturaDataModel
{
    public int FacturaId { get; set; }
    public int ReservaId { get; set; }
    public int? MetodoPagoId { get; set; }
    public decimal Monto { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime? FechaPago { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    public MetodoPagoClienteDataModel? MetodoPago { get; set; }
    public List<DetalleFacturaDataModel> Detalles { get; set; } = new();
}
