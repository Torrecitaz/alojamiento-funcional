using Usuarios.DataAccess.Entities;

namespace Usuarios.DataAccess.Repositories.Interfaces;

public interface IColaboradoresRepository : IRepositoryBase<ColaboradorEntity>
{
    Task<ColaboradorEntity?> GetByUsuarioIdAsync(int usuarioId);
}
