using Microsoft.EntityFrameworkCore;
using Usuarios.DataAccess.Common;
using Usuarios.DataAccess.Contexts;
using Usuarios.DataAccess.Entities;

namespace Usuarios.DataAccess.Queries;

/// <summary>
/// Repositorio de solo lectura para consultas optimizadas de Usuarios.
/// No expone operaciones de escritura.
/// </summary>
public class UsuariosQueryRepository
{
    private readonly UsuariosDbContext _db;

    public UsuariosQueryRepository(UsuariosDbContext db) => _db = db;

    public async Task<PagedResult<UsuarioEntity>> GetPagedAsync(int page, int pageSize, string? search = null)
    {
        var query = _db.Usuarios.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.NombreCompleto.Contains(search) || u.Email.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(u => u.FechaCreacion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<UsuarioEntity>(items, total, page, pageSize);
    }
}
