using Microsoft.EntityFrameworkCore;
using Usuarios.DataAccess.Common;
using Usuarios.DataAccess.Contexts;
using Usuarios.DataAccess.Entities;

namespace Usuarios.DataAccess.Queries;

/// <summary>
/// Repositorio de solo lectura para consultas optimizadas de Clientes.
/// </summary>
public class ClientesQueryRepository
{
    private readonly UsuariosDbContext _db;

    public ClientesQueryRepository(UsuariosDbContext db) => _db = db;

    public async Task<PagedResult<ClienteEntity>> GetPagedAsync(int page, int pageSize, string? nombre = null, string? ciudad = null)
    {
        var query = _db.Clientes.AsNoTracking().Include(c => c.Usuario).AsQueryable();

        if (!string.IsNullOrWhiteSpace(nombre))
            query = query.Where(c => c.Usuario != null && c.Usuario.NombreCompleto.Contains(nombre));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.FechaCreacion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ClienteEntity>(items, total, page, pageSize);
    }
}
