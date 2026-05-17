using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Reservas;

[Table("reservas")]
public class ReservaEntity
{
    [Key]
    public int ReservaId { get; set; }

    public int? DescuentoId { get; set; }
    public int ClienteId { get; set; }       // Ref Lógica a DB_Usuarios
    public int AlojamientoId { get; set; }   // Ref Lógica a DB_Alojamientos

    [Required]
    public DateOnly FechaCheckIn { get; set; }

    [Required]
    public DateOnly FechaCheckOut { get; set; }

    public int NumAdultos { get; set; } = 1;
    public int NumNinos { get; set; } = 0;
    public bool LlevaMascotas { get; set; } = false;
    public int NumHabitaciones { get; set; } = 1;
    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }

    [MaxLength(30)]
    public string Estado { get; set; } = "Pendiente";

    [Required, MaxLength(20)]
    public string CodigoReserva { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }

    // Navegación
    [ForeignKey("DescuentoId")]
    public DescuentoEntity? Descuento { get; set; }
    public ICollection<ReservaDetalleHabitacionEntity> Detalles { get; set; } = new List<ReservaDetalleHabitacionEntity>();
}
