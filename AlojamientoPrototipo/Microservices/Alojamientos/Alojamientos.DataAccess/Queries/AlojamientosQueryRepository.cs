using Microsoft.EntityFrameworkCore;
using Alojamientos.DataAccess.Common;
using Alojamientos.DataAccess.Contexts;
using Alojamientos.DataAccess.Entities;

namespace Alojamientos.DataAccess.Queries;

public class AlojamientosQueryRepository
{
    private readonly AlojamientosDbContext _db;

    public AlojamientosQueryRepository(AlojamientosDbContext db) => _db = db;

    public async Task<PagedResult<AlojamientoEntity>> GetPagedAsync(int page, int pageSize, string? ciudad = null, string? estado = null)
    {
        var query = _db.Alojamientos.AsNoTracking().Include(a => a.TipoAlojamiento).AsQueryable();

        if (!string.IsNullOrWhiteSpace(ciudad))
            query = query.Where(a => a.Ciudad != null && a.Ciudad.Contains(ciudad));

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(a => a.Estado == estado);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.FechaCreacion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AlojamientoEntity>(items, total, page, pageSize);
    }
}
