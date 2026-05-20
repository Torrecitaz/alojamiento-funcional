using Microsoft.EntityFrameworkCore;
using Alojamientos.DataAccess.Common;
using Alojamientos.DataAccess.Contexts;
using Alojamientos.DataAccess.Entities;

namespace Alojamientos.DataAccess.Queries;

public class HabitacionesQueryRepository
{
    private readonly AlojamientosDbContext _db;

    public HabitacionesQueryRepository(AlojamientosDbContext db) => _db = db;

    public async Task<PagedResult<HabitacionEntity>> GetPagedByAlojamientoAsync(int alojamientoId, int page, int pageSize)
    {
        var query = _db.Habitaciones.AsNoTracking().Where(h => h.AlojamientoId == alojamientoId);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(h => h.HabitacionId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<HabitacionEntity>(items, total, page, pageSize);
    }
}
