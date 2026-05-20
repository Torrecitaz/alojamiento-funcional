using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alojamientos.DataAccess.Entities;

[Table("alojamientofotos")]
public class AlojamientoFotoEntity
{
    [Key]
    public int FotoId { get; set; }

    public int AlojamientoId { get; set; }

    [Required, MaxLength(500)]
    public string Url { get; set; } = string.Empty;

    public int Orden { get; set; } = 0;

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    // Navegación
    [ForeignKey("AlojamientoId")]
    public AlojamientoEntity? Alojamiento { get; set; }
}
