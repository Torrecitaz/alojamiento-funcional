namespace Facturacion.DataManagement.Models;

public class AuditoriaGeneralDataModel
{
    public long AuditoriaId { get; set; }
    public string NombreTabla { get; set; } = string.Empty;
    public string Operacion { get; set; } = string.Empty;
    public string RegistroId { get; set; } = string.Empty;
    public string? DatosAnteriores { get; set; }
    public string? DatosNuevos { get; set; }
    public string? UsuarioAccion { get; set; }
    public DateTime FechaAccion { get; set; }
    public string? Origen { get; set; }
}
