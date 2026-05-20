namespace Usuarios.DataManagement.Models;

/// <summary>
/// Modelo intermedio de datos para Usuario.
/// Aísla la entidad de la capa Business.
/// </summary>
public class UsuarioDataModel
{
    public int UsuarioId { get; set; }
    public string Rol { get; set; } = "Cliente";
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public bool Estado { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
