using Usuarios.DataAccess.Entities;
using Usuarios.DataManagement.Models;

namespace Usuarios.DataManagement.Mappers;

public static class UsuariosMapper
{
    public static UsuarioDataModel ToDataModel(UsuarioEntity entity) => new()
    {
        UsuarioId = entity.UsuarioId,
        Rol = entity.Rol,
        Email = entity.Email,
        PasswordHash = entity.PasswordHash,
        NombreCompleto = entity.NombreCompleto,
        Estado = entity.Estado,
        FechaCreacion = entity.FechaCreacion,
        FechaModificacion = entity.FechaModificacion
    };

    public static void UpdateEntity(UsuarioEntity entity, UsuarioDataModel model)
    {
        entity.NombreCompleto = model.NombreCompleto;
        entity.Email = model.Email;
        entity.Estado = model.Estado;
    }
}
