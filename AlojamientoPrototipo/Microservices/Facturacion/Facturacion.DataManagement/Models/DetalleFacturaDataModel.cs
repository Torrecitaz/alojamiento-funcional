namespace Facturacion.DataManagement.Models;

public class DetalleFacturaDataModel
{
    public int DetalleFacturaId { get; set; }
    public int FacturaId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}
