using Reservas.DataAccess.Entities;
using Reservas.DataManagement.Models;

namespace Reservas.DataManagement.Mappers;

public static class ReservasMapper
{
    public static DescuentoDataModel ToDataModel(DescuentoEntity entity) => new()
    {
        DescuentoId = entity.DescuentoId,
        Codigo = entity.Codigo,
        Porcentaje = entity.Porcentaje,
        Activo = entity.Activo
    };

    public static ReservaDetalleHabitacionDataModel ToDataModel(ReservaDetalleHabitacionEntity entity) => new()
    {
        DetalleId = entity.DetalleId,
        ReservaId = entity.ReservaId,
        HabitacionId = entity.HabitacionId,
        PrecioPorNoche = entity.PrecioPorNoche,
        NumNoches = entity.NumNoches,
        SubTotalHabitacion = entity.SubTotalHabitacion
    };

    public static ReservaDataModel ToDataModel(ReservaEntity entity) => new()
    {
        ReservaId = entity.ReservaId,
        DescuentoId = entity.DescuentoId,
        ClienteId = entity.ClienteId,
        AlojamientoId = entity.AlojamientoId,
        FechaCheckIn = entity.FechaCheckIn,
        FechaCheckOut = entity.FechaCheckOut,
        NumAdultos = entity.NumAdultos,
        NumNinos = entity.NumNinos,
        LlevaMascotas = entity.LlevaMascotas,
        NumHabitaciones = entity.NumHabitaciones,
        SubTotal = entity.SubTotal,
        Total = entity.Total,
        Estado = entity.Estado,
        CodigoReserva = entity.CodigoReserva,
        FechaCreacion = entity.FechaCreacion,
        FechaModificacion = entity.FechaModificacion,
        Descuento = entity.Descuento != null ? ToDataModel(entity.Descuento) : null,
        DetallesHabitacion = entity.DetallesHabitacion?.Select(ToDataModel).ToList() ?? new List<ReservaDetalleHabitacionDataModel>()
    };
}
