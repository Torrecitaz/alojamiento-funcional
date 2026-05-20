using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.DataAccess.Entities;

[Table("facturas")]
public class FacturaEntity
{
    [Key]
    public int FacturaId { get; set; }

    public int ReservaId { get; set; } // Ref Lógica a DB_Reservas.Reservas

    public int? MetodoPagoId { get; set; }

    public decimal Monto { get; set; }

    [MaxLength(30)]
    public string Estado { get; set; } = "Aprobado";

    public DateTime? FechaPago { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    
    public DateTime? FechaModificacion { get; set; }

    // Navegación
    [ForeignKey("MetodoPagoId")]
    public MetodoPagoClienteEntity? MetodoPago { get; set; }

    public ICollection<DetalleFacturaEntity> Detalles { get; set; } = new List<DetalleFacturaEntity>();
}
