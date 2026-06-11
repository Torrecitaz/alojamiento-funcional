namespace Alojamientos.DataManagement.Models;

public class TipoAlojamientoDataModel
{
    public int TipoAlojamientoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

public class AlojamientoFotoDataModel
{
    public int FotoId { get; set; }
    public int AlojamientoId { get; set; }
    public string Url { get; set; } = string.Empty;
    public int Orden { get; set; }
    public string? Descripcion { get; set; }
}

public class AlojamientoDataModel
{
    public int AlojamientoId { get; set; }
    public int SocioId { get; set; }
    public int TipoAlojamientoId { get; set; }
    public string? Ciudad { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public int? Estrellas { get; set; }
    public decimal CalificacionPromedio { get; set; }
    public int TotalResenas { get; set; }
    public bool AdmiteMascotas { get; set; }
    public bool TienePiscina { get; set; }
    public bool TieneParqueadero { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    public string? Provincia { get; set; }
    public string? Pais { get; set; }
    public string? Politicas { get; set; }
    public string? CheckInTime { get; set; }
    public string? CheckOutTime { get; set; }
    public string? Servicios { get; set; }
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }

    public TipoAlojamientoDataModel? TipoAlojamiento { get; set; }
    public List<AlojamientoFotoDataModel> Fotos { get; set; } = new();
}

public class CalendarioDisponibilidadDataModel
{
    public int CalendarioId { get; set; }
    public int HabitacionId { get; set; }
    public DateOnly Fecha { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime? FechaModificacion { get; set; }
    public string? ReservaId { get; set; }
    public string Origen { get; set; } = "ALOJAEXPRESS";
}

public class HabitacionDataModel
{
    public int HabitacionId { get; set; }
    public int AlojamientoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CapacidadAdultos { get; set; }
    public int CapacidadNinos { get; set; }
    public int NumBanos { get; set; }
    public int NumDormitorios { get; set; }
    public bool TieneCocina { get; set; }
    public bool TieneAireAcondicionado { get; set; }
    public decimal? SuperficieM2 { get; set; }
    public decimal PrecioNoche { get; set; }
    public string Estado { get; set; } = "Activo";
    public string? Fotos { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
