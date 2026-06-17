using System;

namespace Usuarios.DataManagement.Models;

public class ColaboradorDataModel
{
    public int ColaboradorId { get; set; }
    public int? UsuarioId { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    public UsuarioDataModel? Usuario { get; set; }
}
