namespace Reservas.DataManagement.Models;

public class DescuentoDataModel
{
    public int DescuentoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public decimal Porcentaje { get; set; }
    public bool Activo { get; set; }
}

public class ReservaDetalleHabitacionDataModel
{
    public int DetalleId { get; set; }
    public int ReservaId { get; set; }
    public int HabitacionId { get; set; }
    public decimal PrecioPorNoche { get; set; }
    public int NumNoches { get; set; }
    public decimal SubTotalHabitacion { get; set; }
}

public class ReservaDataModel
{
    public int ReservaId { get; set; }
    public int? DescuentoId { get; set; }
    public int ClienteId { get; set; }
    public int AlojamientoId { get; set; }
    public DateOnly FechaCheckIn { get; set; }
    public DateOnly FechaCheckOut { get; set; }
    public int NumAdultos { get; set; }
    public int NumNinos { get; set; }
    public bool LlevaMascotas { get; set; }
    public int NumHabitaciones { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string CodigoReserva { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    public DescuentoDataModel? Descuento { get; set; }
    public List<ReservaDetalleHabitacionDataModel> DetallesHabitacion { get; set; } = new();
}
