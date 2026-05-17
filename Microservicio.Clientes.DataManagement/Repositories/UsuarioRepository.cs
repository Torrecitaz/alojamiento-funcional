using Microservicio.Cliente.DatAccess.Contexts;
using Microservicio.Cliente.DatAccess.Entities.Usuarios;
using Microservicio.Clientes.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Clientes.DataManagement.Repositories;

public class UsuarioRepository : Repository<UsuarioEntity>, IUsuarioRepository
{
    private readonly UsuariosDbContext _db;

    public UsuarioRepository(UsuariosDbContext context) : base(context)
    {
        _db = context;
    }

    public async Task<UsuarioEntity?> GetByEmailAsync(string email)
        => await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
}
