using Alojamientos.DataAccess.Common;
using Alojamientos.DataAccess.Contexts;
using Alojamientos.DataAccess.Entities;
using Alojamientos.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public async Task<bool> ExistsBloqueoOcupacionWithLockAsync(int habitacionId, DateOnly fechaInicio, DateOnly fechaFin)
    {
        // Bloquear pesimistamente la fila de la habitación
        await _context.Database.ExecuteSqlRawAsync(
            "SELECT 1 FROM habitaciones WHERE habitacionid = {0} FOR UPDATE", 
            habitacionId);

        // Verificar si existen días ocupados o bloqueados en el rango
        var anyOccupied = await _context.Set<CalendarioDisponibilidadEntity>()
            .AnyAsync(c => c.HabitacionId == habitacionId &&
                           c.Fecha >= fechaInicio &&
                           c.Fecha <= fechaFin &&
                           (c.Estado == "Ocupado" || c.Estado == "Bloqueado"));

        return anyOccupied;
    }
}
