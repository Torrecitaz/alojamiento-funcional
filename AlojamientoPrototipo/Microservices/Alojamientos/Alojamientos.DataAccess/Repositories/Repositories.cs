using Alojamientos.DataAccess.Common;
using Alojamientos.DataAccess.Contexts;
using Alojamientos.DataAccess.Entities;
using Alojamientos.DataAccess.Repositories.Interfaces;

namespace Alojamientos.DataAccess.Repositories;

public class AlojamientosRepository : RepositoryBase<AlojamientoEntity>, IAlojamientosRepository
{
    public AlojamientosRepository(AlojamientosDbContext context) : base(context) { }
}

public class HabitacionesRepository : RepositoryBase<HabitacionEntity>, IHabitacionesRepository
{
    public HabitacionesRepository(AlojamientosDbContext context) : base(context) { }
}

public class TiposAlojamientoRepository : RepositoryBase<TipoAlojamientoEntity>, ITiposAlojamientoRepository
{
    public TiposAlojamientoRepository(AlojamientosDbContext context) : base(context) { }
}

public class AlojamientoFotosRepository : RepositoryBase<AlojamientoFotoEntity>, IAlojamientoFotosRepository
{
    public AlojamientoFotosRepository(AlojamientosDbContext context) : base(context) { }
}

public class CalendarioDisponibilidadRepository : RepositoryBase<CalendarioDisponibilidadEntity>, ICalendarioDisponibilidadRepository
{
    public CalendarioDisponibilidadRepository(AlojamientosDbContext context) : base(context) { }
}
