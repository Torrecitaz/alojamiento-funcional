using Reservas.Business.DTOs;
using Reservas.DataManagement.Models;

namespace Reservas.Business.Mappers;

public static class ReservasBusinessMapper
{
    public static ReservaResponse ToResponse(ReservaDataModel model) => new(
        model.ReservaId,
        model.ClienteId,
        model.AlojamientoId,
        model.FechaCheckIn,
        model.FechaCheckOut,
        model.NumAdultos,
        model.NumNinos,
        model.LlevaMascotas,
        model.NumHabitaciones,
        model.SubTotal,
        model.Total,
        model.Estado,
        model.CodigoReserva,
        model.FechaCreacion,
        model.DetallesHabitacion.Select(ToResponse).ToList(),
        model.Descuento?.Codigo,
        model.Descuento?.Porcentaje
    );

    public static ReservaResumenResponse ToResumenResponse(ReservaDataModel model) => new(
        model.ReservaId,
        model.ClienteId,
        model.AlojamientoId,
        model.FechaCheckIn,
        model.FechaCheckOut,
        model.NumHabitaciones,
        model.Total,
        model.Estado,
        model.CodigoReserva,
        model.FechaCreacion
    );

    public static DetalleHabitacionResponse ToResponse(ReservaDetalleHabitacionDataModel model) => new(
        model.DetalleId,
        model.HabitacionId,
        model.PrecioPorNoche,
        model.NumNoches,
        model.SubTotalHabitacion
    );
}
