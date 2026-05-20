using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alojamientos.DataAccess.Entities;

[Table("calendariodisponibilidad")]
public class CalendarioDisponibilidadEntity
{
    [Key]
    public int CalendarioId { get; set; }

    public int HabitacionId { get; set; }

    [Required]
    public DateOnly Fecha { get; set; }

    [Required, MaxLength(20)]
    public string Estado { get; set; } = "Disponible"; // Disponible, Ocupado, Bloqueado

    public DateTime? FechaModificacion { get; set; }

    // Navegación
    [ForeignKey("HabitacionId")]
    public HabitacionEntity? Habitacion { get; set; }
}
