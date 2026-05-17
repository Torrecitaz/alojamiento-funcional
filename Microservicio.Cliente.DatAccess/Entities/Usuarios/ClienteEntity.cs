using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Usuarios;

[Table("clientes")]
public class ClienteEntity
{
    [Key]
    public int ClienteId { get; set; }

    public int? UsuarioId { get; set; }

    [Required, MaxLength(20)]
    public string Cedula { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? FotoUrl { get; set; }

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(300)]
    public string? Domicilio { get; set; }

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    public int TotalReservas { get; set; } = 0;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }

    // Navegación
    [ForeignKey("UsuarioId")]
    public UsuarioEntity? Usuario { get; set; }
}
