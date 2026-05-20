using Usuarios.DataAccess.Common;
using Usuarios.DataAccess.Contexts;
using Usuarios.DataAccess.Entities;
using Usuarios.DataAccess.Repositories.Interfaces;

namespace Usuarios.DataAccess.Repositories;

public class LocalizacionesRepository : RepositoryBase<LocalizacionEntity>, ILocalizacionesRepository
{
    public LocalizacionesRepository(UsuariosDbContext context) : base(context) { }
}
