namespace Usuarios.DataManagement.Models;

public class ClienteDataModel
{
    public int ClienteId { get; set; }
    public int? UsuarioId { get; set; }
    public string Cedula { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public string? Telefono { get; set; }
    public string? Domicilio { get; set; }
    public string Email { get; set; } = string.Empty;
    public int TotalReservas { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    // Datos del usuario asociado (desnormalizado para evitar dependencia directa)
    public UsuarioDataModel? Usuario { get; set; }
}
