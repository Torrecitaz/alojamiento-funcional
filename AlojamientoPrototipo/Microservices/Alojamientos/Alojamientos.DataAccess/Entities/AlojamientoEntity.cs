using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alojamientos.DataAccess.Entities;

[Table("alojamientos")]
public class AlojamientoEntity
{
    [Key]
    public int AlojamientoId { get; set; }

    public int SocioId { get; set; } // Ref lógica a Usuarios.API (Propietario)

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
    [MaxLength(100)]
    public string? Provincia { get; set; }

    [MaxLength(100)]
    public string? Pais { get; set; }

    public string? Politicas { get; set; }

    [MaxLength(50)]
    public string? CheckInTime { get; set; }

    [MaxLength(50)]
    public string? CheckOutTime { get; set; }

    public string? Servicios { get; set; }

    public NpgsqlTypes.NpgsqlPoint? Coordenadas { get; set; }

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
