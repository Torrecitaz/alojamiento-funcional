using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Facturacion;

[Table("metodospagocliente")]
public class MetodoPagoEntity
{
    [Key]
    public int MetodoPagoId { get; set; }

    [Required, MaxLength(30)]
    public string Tipo { get; set; } = string.Empty; // DEBITO, CREDITO, EnSitio

    // Navegación
    public ICollection<FacturaEntity> Facturas { get; set; } = new List<FacturaEntity>();
}
