using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.DataAccess.Entities;

[Table("metodospagocliente")]
public class MetodoPagoClienteEntity
{
    [Key]
    public int MetodoPagoId { get; set; }

    [Required, MaxLength(30)]
    public string Tipo { get; set; } = string.Empty; // DEBITO, CREDITO, EnSitio

    // ID externo (ej: UUID de Booking) para resolución sin traducción manual
    public Guid? ExternalId { get; set; }

    // Navegación
    public ICollection<FacturaEntity> Facturas { get; set; } = new List<FacturaEntity>();
}
