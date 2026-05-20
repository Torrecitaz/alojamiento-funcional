using Microsoft.EntityFrameworkCore;
using Reservas.DataAccess.Common;
using Reservas.DataAccess.Contexts;
using Reservas.DataAccess.Entities;

namespace Reservas.DataAccess.Queries;

public class ReservasQueryRepository
{
    private readonly ReservasDbContext _db;

    public ReservasQueryRepository(ReservasDbContext db) => _db = db;

    public async Task<PagedResult<ReservaEntity>> GetPagedByClienteAsync(int clienteId, int page, int pageSize)
    {
        var query = _db.Reservas.AsNoTracking().Where(r => r.ClienteId == clienteId);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.FechaCreacion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ReservaEntity>(items, total, page, pageSize);
    }
}
