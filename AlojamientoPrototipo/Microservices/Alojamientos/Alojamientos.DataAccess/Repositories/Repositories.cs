using Alojamientos.DataAccess.Common;
using Alojamientos.DataAccess.Contexts;
using Alojamientos.DataAccess.Entities;
using Alojamientos.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Alojamientos.DataAccess.Repositories;

public class AlojamientosRepository : RepositoryBase<AlojamientoEntity>, IAlojamientosRepository
{
    public AlojamientosRepository(AlojamientosDbContext context) : base(context) { }

    public async Task<(IEnumerable<AlojamientoEntity> Items, int TotalRecords)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? ciudad = null,
        string? tipo = null,
        int? estrellas = null,
        bool? admiteMascotas = null,
        bool? tienePiscina = null)
    {
        var query = _context.Set<AlojamientoEntity>()
            .Include(a => a.TipoAlojamiento)
            .Include(a => a.Fotos)
            .Include(a => a.Habitaciones)
            .Where(a => a.Estado != "Inactivo" && a.Estado != "Inactiva");

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a => EF.Functions.ILike(a.Nombre, $"%{search}%") || 
                                     (a.Descripcion != null && EF.Functions.ILike(a.Descripcion, $"%{search}%")));
        }

        if (!string.IsNullOrWhiteSpace(ciudad))
        {
            query = query.Where(a => a.Ciudad != null && EF.Functions.ILike(a.Ciudad, $"%{ciudad}%"));
        }

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            query = query.Where(a => a.TipoAlojamiento != null && EF.Functions.ILike(a.TipoAlojamiento.Nombre, tipo));
        }

        if (estrellas.HasValue)
        {
            query = query.Where(a => a.Estrellas >= estrellas.Value);
        }

        if (admiteMascotas.HasValue)
        {
            query = query.Where(a => a.AdmiteMascotas == admiteMascotas.Value);
        }

        if (tienePiscina.HasValue)
        {
            query = query.Where(a => a.TienePiscina == tienePiscina.Value);
        }

        int totalRecords = await query.CountAsync();

        var items = await query
            .OrderBy(a => a.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalRecords);
    }
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
