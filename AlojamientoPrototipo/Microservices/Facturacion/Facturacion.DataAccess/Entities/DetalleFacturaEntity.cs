using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.DataAccess.Entities;

[Table("detallefacturas")]
public class DetalleFacturaEntity
{
    [Key]
    public int DetalleFacturaId { get; set; }

    public int FacturaId { get; set; }

    [Required, MaxLength(200)]
    public string Descripcion { get; set; } = string.Empty;

    public int Cantidad { get; set; } = 1;

    public decimal PrecioUnitario { get; set; }

    // Navegación
    [ForeignKey("FacturaId")]
    public FacturaEntity? Factura { get; set; }
}
