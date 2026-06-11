using Alojamientos.DataAccess.Entities;
using Alojamientos.DataManagement.Models;

namespace Alojamientos.DataManagement.Mappers;

public static class AlojamientosMapper
{
    public static TipoAlojamientoDataModel ToDataModel(TipoAlojamientoEntity entity) => new()
    {
        TipoAlojamientoId = entity.TipoAlojamientoId,
        Nombre = entity.Nombre,
        Descripcion = entity.Descripcion
    };

    public static AlojamientoFotoDataModel ToDataModel(AlojamientoFotoEntity entity) => new()
    {
        FotoId = entity.FotoId,
        AlojamientoId = entity.AlojamientoId,
        Url = entity.Url,
        Orden = entity.Orden,
        Descripcion = entity.Descripcion
    };

    public static AlojamientoDataModel ToDataModel(AlojamientoEntity entity) => new()
    {
        AlojamientoId = entity.AlojamientoId,
        SocioId = entity.SocioId,
        TipoAlojamientoId = entity.TipoAlojamientoId,
        Ciudad = entity.Ciudad,
        Nombre = entity.Nombre,
        Descripcion = entity.Descripcion,
        Direccion = entity.Direccion,
        Estrellas = entity.Estrellas,
        CalificacionPromedio = entity.CalificacionPromedio,
        TotalResenas = entity.TotalResenas,
        AdmiteMascotas = entity.AdmiteMascotas,
        TienePiscina = entity.TienePiscina,
        TieneParqueadero = entity.TieneParqueadero,
        Estado = entity.Estado,
        FechaCreacion = entity.FechaCreacion,
        FechaModificacion = entity.FechaModificacion,
        Provincia = entity.Provincia,
        Pais = entity.Pais,
        Politicas = entity.Politicas,
        CheckInTime = entity.CheckInTime,
        CheckOutTime = entity.CheckOutTime,
        Servicios = entity.Servicios,
        Latitud = entity.Coordenadas?.X,
        Longitud = entity.Coordenadas?.Y,
        TipoAlojamiento = entity.TipoAlojamiento != null ? ToDataModel(entity.TipoAlojamiento) : null,
        Fotos = entity.Fotos?.Select(ToDataModel).ToList() ?? new List<AlojamientoFotoDataModel>()
    };

    public static HabitacionDataModel ToDataModel(HabitacionEntity entity) => new()
    {
        HabitacionId = entity.HabitacionId,
        AlojamientoId = entity.AlojamientoId,
        Nombre = entity.Nombre,
        Descripcion = entity.Descripcion,
        CapacidadAdultos = entity.CapacidadAdultos,
        CapacidadNinos = entity.CapacidadNinos,
        NumBanos = entity.NumBanos,
        NumDormitorios = entity.NumDormitorios,
        TieneCocina = entity.TieneCocina,
        TieneAireAcondicionado = entity.TieneAireAcondicionado,
        SuperficieM2 = entity.SuperficieM2,
        PrecioNoche = entity.PrecioNoche,
        Estado = entity.Estado,
        Fotos = entity.Fotos,
        FechaModificacion = entity.FechaModificacion
    };

    public static CalendarioDisponibilidadDataModel ToDataModel(CalendarioDisponibilidadEntity entity) => new()
    {
        CalendarioId = entity.CalendarioId,
        HabitacionId = entity.HabitacionId,
        Fecha = entity.Fecha,
        Estado = entity.Estado,
        FechaModificacion = entity.FechaModificacion,
        ReservaId = entity.ReservaId,
        Origen = entity.Origen
    };

    public static void UpdateEntity(AlojamientoEntity entity, AlojamientoDataModel model)
    {
        entity.Nombre = model.Nombre;
        entity.Ciudad = model.Ciudad;
        entity.Direccion = model.Direccion;
        entity.Descripcion = model.Descripcion;
        entity.Estrellas = model.Estrellas;
        entity.AdmiteMascotas = model.AdmiteMascotas;
        entity.TienePiscina = model.TienePiscina;
        entity.TieneParqueadero = model.TieneParqueadero;
        entity.TipoAlojamientoId = model.TipoAlojamientoId;
        entity.Estado = model.Estado;
        entity.Provincia = model.Provincia;
        entity.Pais = model.Pais;
        entity.Politicas = model.Politicas;
        entity.CheckInTime = model.CheckInTime;
        entity.CheckOutTime = model.CheckOutTime;
        entity.Servicios = model.Servicios;
        
        if (model.Latitud.HasValue && model.Longitud.HasValue)
        {
            entity.Coordenadas = new NpgsqlTypes.NpgsqlPoint(model.Latitud.Value, model.Longitud.Value);
        }
        else
        {
            entity.Coordenadas = null;
        }

        entity.FechaModificacion = DateTime.UtcNow;
    }

    public static void UpdateEntity(HabitacionEntity entity, HabitacionDataModel model)
    {
        entity.Nombre = model.Nombre;
        entity.Descripcion = model.Descripcion;
        entity.CapacidadAdultos = model.CapacidadAdultos;
        entity.CapacidadNinos = model.CapacidadNinos;
        entity.NumBanos = model.NumBanos;
        entity.NumDormitorios = model.NumDormitorios;
        entity.TieneCocina = model.TieneCocina;
        entity.TieneAireAcondicionado = model.TieneAireAcondicionado;
        entity.SuperficieM2 = model.SuperficieM2;
        entity.PrecioNoche = model.PrecioNoche;
        entity.Estado = model.Estado;
        entity.Fotos = model.Fotos;
        entity.FechaModificacion = DateTime.UtcNow;
    }
}
