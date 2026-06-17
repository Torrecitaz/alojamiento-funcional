using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Usuarios.DataAccess.Entities;

[Table("colaboradores")]
public class ColaboradorEntity
{
    [Key]
    public int ColaboradorId { get; set; }

    public int? UsuarioId { get; set; }

    [Required, MaxLength(200)]
    public string NombreEmpresa { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Telefono { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }

    // Navegación
    [ForeignKey("UsuarioId")]
    public UsuarioEntity? Usuario { get; set; }
}
