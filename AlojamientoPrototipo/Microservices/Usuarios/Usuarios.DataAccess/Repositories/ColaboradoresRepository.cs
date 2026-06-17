using Microsoft.EntityFrameworkCore;
using Usuarios.DataAccess.Common;
using Usuarios.DataAccess.Contexts;
using Usuarios.DataAccess.Entities;
using Usuarios.DataAccess.Repositories.Interfaces;

namespace Usuarios.DataAccess.Repositories;

public class ColaboradoresRepository : RepositoryBase<ColaboradorEntity>, IColaboradoresRepository
{
    private readonly UsuariosDbContext _db;

    public ColaboradoresRepository(UsuariosDbContext context) : base(context)
    {
        _db = context;
    }

    public async Task<ColaboradorEntity?> GetByUsuarioIdAsync(int usuarioId)
        => await _db.Colaboradores.Include(c => c.Usuario).FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
}
