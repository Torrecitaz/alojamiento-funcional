using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reservas.DataAccess.Entities;

[Table("descuentos")]
public class DescuentoEntity
{
    [Key]
    public int DescuentoId { get; set; }

    [Required, MaxLength(20)]
    public string Codigo { get; set; } = string.Empty;

    public decimal Porcentaje { get; set; }

    public bool Activo { get; set; } = true;

    // Navegación
    public ICollection<ReservaEntity> Reservas { get; set; } = new List<ReservaEntity>();
}
