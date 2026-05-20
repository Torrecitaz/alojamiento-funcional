using Microsoft.EntityFrameworkCore;
using Usuarios.DataAccess.Common;
using Usuarios.DataAccess.Contexts;
using Usuarios.DataAccess.Entities;

namespace Usuarios.DataAccess.Queries;

/// <summary>
/// Repositorio de solo lectura para consultas de Localizaciones.
/// </summary>
public class LocalizacionesQueryRepository
{
    private readonly UsuariosDbContext _db;

    public LocalizacionesQueryRepository(UsuariosDbContext db) => _db = db;

    public async Task<PagedResult<LocalizacionEntity>> GetPagedAsync(int page, int pageSize)
    {
        var total = await _db.Localizaciones.CountAsync();
        var items = await _db.Localizaciones.AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<LocalizacionEntity>(items, total, page, pageSize);
    }
}
