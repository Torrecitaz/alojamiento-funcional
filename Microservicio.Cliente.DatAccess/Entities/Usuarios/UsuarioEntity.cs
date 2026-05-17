using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Usuarios;

[Table("usuarios")]
public class UsuarioEntity
{
    [Key]
    public int UsuarioId { get; set; }

    [Required, MaxLength(10)]
    public string Rol { get; set; } = "Cliente";

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string NombreCompleto { get; set; } = string.Empty;

    public bool Estado { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }

    // Navegación
    public ClienteEntity? Cliente { get; set; }
}
