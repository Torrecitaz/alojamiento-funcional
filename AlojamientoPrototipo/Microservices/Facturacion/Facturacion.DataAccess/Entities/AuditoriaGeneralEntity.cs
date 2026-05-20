using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Facturacion.DataAccess.Entities;

[Table("auditoriageneral")]
public class AuditoriaGeneralEntity
{
    [Key]
    public long AuditoriaId { get; set; }

    [Required, MaxLength(100)]
    public string NombreTabla { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    public string Operacion { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string RegistroId { get; set; } = string.Empty;

    public string? DatosAnteriores { get; set; }

    public string? DatosNuevos { get; set; }

    [MaxLength(100)]
    public string? UsuarioAccion { get; set; }

    public DateTime FechaAccion { get; set; } = DateTime.UtcNow;

    [MaxLength(45)]
    public string? Origen { get; set; }
}
