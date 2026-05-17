using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Alojamientos;

[Table("alojamientos")]
public class AlojamientoEntity
{
    [Key]
    public int AlojamientoId { get; set; }

    public int SocioId { get; set; } // Ref Lógica a DB_Usuarios

    public int TipoAlojamientoId { get; set; }

    [MaxLength(100)]
    public string? Ciudad { get; set; }

    [Required, MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    [Required, MaxLength(300)]
    public string Direccion { get; set; } = string.Empty;

    public int? Estrellas { get; set; }
    public decimal CalificacionPromedio { get; set; } = 0;
    public int TotalResenas { get; set; } = 0;
    public bool AdmiteMascotas { get; set; } = false;
    public bool TienePiscina { get; set; } = false;
    public bool TieneParqueadero { get; set; } = false;

    [MaxLength(20)]
    public string Estado { get; set; } = "Pendiente";

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }

    // Navegación
    [ForeignKey("TipoAlojamientoId")]
    public TipoAlojamientoEntity? TipoAlojamiento { get; set; }
    public ICollection<AlojamientoFotoEntity> Fotos { get; set; } = new List<AlojamientoFotoEntity>();
    public ICollection<HabitacionEntity> Habitaciones { get; set; } = new List<HabitacionEntity>();
}
