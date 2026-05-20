using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reservas.DataAccess.Entities;

[Table("reservadetallehabitacion")]
public class ReservaDetalleHabitacionEntity
{
    [Key]
    public int DetalleId { get; set; }

    public int ReservaId { get; set; }

    public int HabitacionId { get; set; } // Ref Lógica a DB_Alojamientos.Habitaciones

    public decimal PrecioPorNoche { get; set; }

    public int NumNoches { get; set; }

    public decimal SubTotalHabitacion { get; set; }

    // Navegación
    [ForeignKey("ReservaId")]
    public ReservaEntity? Reserva { get; set; }
}
