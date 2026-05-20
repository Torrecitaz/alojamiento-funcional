using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alojamientos.DataAccess.Entities;

[Table("tiposalojamiento")]
public class TipoAlojamientoEntity
{
    [Key]
    public int TipoAlojamientoId { get; set; }

    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    // Navegación
    public ICollection<AlojamientoEntity> Alojamientos { get; set; } = new List<AlojamientoEntity>();
}
