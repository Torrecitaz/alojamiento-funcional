using Microsoft.EntityFrameworkCore;
using Usuarios.DataAccess.Common;
using Usuarios.DataAccess.Contexts;
using Usuarios.DataAccess.Entities;
using Usuarios.DataAccess.Repositories.Interfaces;

namespace Usuarios.DataAccess.Repositories;

public class ClientesRepository : RepositoryBase<ClienteEntity>, IClientesRepository
{
    private readonly UsuariosDbContext _db;

    public ClientesRepository(UsuariosDbContext context) : base(context)
    {
        _db = context;
    }

    public async Task<ClienteEntity?> GetByCedulaAsync(string cedula)
        => await _db.Clientes.Include(c => c.Usuario).FirstOrDefaultAsync(c => c.Cedula == cedula);

    public async Task<ClienteEntity?> GetByUsuarioIdAsync(int usuarioId)
        => await _db.Clientes.Include(c => c.Usuario).FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

    /// <summary>
    /// Ejecuta el Stored Procedure sp_registrar_cliente directamente en PostgreSQL.
    /// Crea el Usuario y el Cliente en una sola transacción atómica.
    /// </summary>
    public async Task RegistrarClienteSPAsync(string email, string password, string nombre, string cedula, string telefono, string domicilio)
    {
        await _db.Database.ExecuteSqlRawAsync(
            "CALL sp_registrar_cliente({0}, {1}, {2}, {3}, {4}, {5})",
            email, password, nombre, cedula, telefono, domicilio);
    }
}
