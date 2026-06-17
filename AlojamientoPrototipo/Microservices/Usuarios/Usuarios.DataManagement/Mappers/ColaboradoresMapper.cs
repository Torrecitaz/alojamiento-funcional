using Usuarios.DataAccess.Entities;
using Usuarios.DataManagement.Models;

namespace Usuarios.DataManagement.Mappers;

public static class ColaboradoresMapper
{
    public static ColaboradorDataModel ToDataModel(ColaboradorEntity entity) => new()
    {
        ColaboradorId = entity.ColaboradorId,
        UsuarioId = entity.UsuarioId,
        NombreEmpresa = entity.NombreEmpresa,
        Telefono = entity.Telefono,
        FechaCreacion = entity.FechaCreacion,
        FechaModificacion = entity.FechaModificacion,
        Usuario = entity.Usuario != null ? UsuariosMapper.ToDataModel(entity.Usuario) : null
    };

    public static void UpdateEntity(ColaboradorEntity entity, ColaboradorDataModel model)
    {
        entity.NombreEmpresa = model.NombreEmpresa;
        entity.Telefono = model.Telefono;
    }
}
