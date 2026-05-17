using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Alojamientos;

[Table("habitaciones")]
public class HabitacionEntity
{
    [Key]
    public int HabitacionId { get; set; }

    public int AlojamientoId { get; set; }

    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    public int CapacidadAdultos { get; set; } = 2;
    public int CapacidadNinos { get; set; } = 0;
    public int NumBanos { get; set; } = 1;
    public int NumDormitorios { get; set; } = 1;
    public bool TieneCocina { get; set; } = false;
    public bool TieneAireAcondicionado { get; set; } = false;
    public decimal? SuperficieM2 { get; set; }
    public decimal PrecioNoche { get; set; } = 0;
    public DateTime? FechaModificacion { get; set; }

    // Navegación
    [ForeignKey("AlojamientoId")]
    public AlojamientoEntity? Alojamiento { get; set; }
    public ICollection<CalendarioDisponibilidadEntity> Calendario { get; set; } = new List<CalendarioDisponibilidadEntity>();
}
