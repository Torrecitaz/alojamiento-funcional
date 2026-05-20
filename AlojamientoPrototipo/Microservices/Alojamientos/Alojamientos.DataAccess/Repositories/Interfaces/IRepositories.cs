using Alojamientos.DataAccess.Entities;

namespace Alojamientos.DataAccess.Repositories.Interfaces;

public interface IAlojamientosRepository : IRepositoryBase<AlojamientoEntity>
{
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
}
