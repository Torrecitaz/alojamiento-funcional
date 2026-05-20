using Microsoft.EntityFrameworkCore;
using Usuarios.DataAccess.Common;
using Usuarios.DataAccess.Contexts;
using Usuarios.DataAccess.Entities;
using Usuarios.DataAccess.Repositories.Interfaces;

namespace Usuarios.DataAccess.Repositories;

public class UsuariosRepository : RepositoryBase<UsuarioEntity>, IUsuariosRepository
{
    private readonly UsuariosDbContext _db;

    public UsuariosRepository(UsuariosDbContext context) : base(context)
    {
        _db = context;
    }

    public async Task<UsuarioEntity?> GetByEmailAsync(string email)
        => await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
}
