using Alojamientos.Business.DTOs.Alojamientos;
using Alojamientos.Business.DTOs.Fotos;
using Alojamientos.Business.DTOs.Habitaciones;
using Alojamientos.DataManagement.Models;

namespace Alojamientos.Business.Mappers;

public static class AlojamientosBusinessMapper
{
    public static AlojamientoResponse ToResponse(AlojamientoDataModel model) => new(
        model.AlojamientoId,
        model.SocioId,
        model.TipoAlojamientoId,
        model.TipoAlojamiento?.Nombre ?? string.Empty,
        model.Nombre,
        model.Ciudad,
        model.Direccion,
        model.Descripcion,
        model.Estrellas,
        model.CalificacionPromedio,
        model.TotalResenas,
        model.AdmiteMascotas,
        model.TienePiscina,
        model.TieneParqueadero,
        model.Estado,
        model.FechaCreacion
    );

    public static HabitacionResponse ToResponse(HabitacionDataModel model) => new(
        model.HabitacionId,
        model.AlojamientoId,
        model.Nombre,
        model.Descripcion,
        model.CapacidadAdultos,
        model.CapacidadNinos,
        model.NumBanos,
        model.NumDormitorios,
        model.TieneCocina,
        model.TieneAireAcondicionado,
        model.SuperficieM2,
        model.PrecioNoche
    );

    public static FotoResponse ToResponse(AlojamientoFotoDataModel model) => new(
        model.FotoId,
        model.AlojamientoId,
        model.Url,
        model.Orden,
        model.Descripcion
    );
}
