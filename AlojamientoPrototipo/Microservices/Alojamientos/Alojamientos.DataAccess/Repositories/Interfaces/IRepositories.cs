using Alojamientos.DataAccess.Entities;

namespace Alojamientos.DataAccess.Repositories.Interfaces;

public interface IAlojamientosRepository : IRepositoryBase<AlojamientoEntity>
{
    Task<(IEnumerable<AlojamientoEntity> Items, int TotalRecords)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? ciudad = null,
        string? tipo = null,
        int? estrellas = null,
        bool? admiteMascotas = null,
        bool? tienePiscina = null);
}

public interface IHabitacionesRepository : IRepositoryBase<HabitacionEntity>
{
}

public interface ITiposAlojamientoRepository : IRepositoryBase<TipoAlojamientoEntity>
{
}

public interface IAlojamientoFotosRepository : IRepositoryBase<AlojamientoFotoEntity>
{
}

public interface ICalendarioDisponibilidadRepository : IRepositoryBase<CalendarioDisponibilidadEntity>
{
    Task<bool> ExistsBloqueoOcupacionWithLockAsync(int habitacionId, DateOnly fechaInicio, DateOnly fechaFin);
}
