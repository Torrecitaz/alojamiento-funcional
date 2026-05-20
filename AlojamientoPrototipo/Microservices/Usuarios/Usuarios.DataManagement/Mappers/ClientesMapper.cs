using Usuarios.DataAccess.Entities;
using Usuarios.DataManagement.Models;

namespace Usuarios.DataManagement.Mappers;

public static class ClientesMapper
{
    public static ClienteDataModel ToDataModel(ClienteEntity entity) => new()
    {
        ClienteId = entity.ClienteId,
        UsuarioId = entity.UsuarioId,
        Cedula = entity.Cedula,
        FotoUrl = entity.FotoUrl,
        Telefono = entity.Telefono,
        Domicilio = entity.Domicilio,
        Email = entity.Email,
        TotalReservas = entity.TotalReservas,
        FechaCreacion = entity.FechaCreacion,
        FechaModificacion = entity.FechaModificacion,
        Usuario = entity.Usuario != null ? UsuariosMapper.ToDataModel(entity.Usuario) : null
    };

    public static void UpdateEntity(ClienteEntity entity, ClienteDataModel model)
    {
        entity.Telefono = model.Telefono;
        entity.Domicilio = model.Domicilio;
        entity.FotoUrl = model.FotoUrl;
    }
}
