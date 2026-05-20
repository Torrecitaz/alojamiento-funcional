using Usuarios.Business.DTOs.Clientes;
using Usuarios.Business.DTOs.Usuarios;
using Usuarios.DataManagement.Models;

namespace Usuarios.Business.Mappers;

public static class ClientesBusinessMapper
{
    public static ClienteResponse ToResponse(ClienteDataModel model)
    {
        UsuarioResponse? usuario = model.Usuario != null
            ? UsuariosBusinessMapper.ToResponse(model.Usuario)
            : null;

        return new ClienteResponse(
            model.ClienteId, model.UsuarioId, model.Cedula, model.FotoUrl,
            model.Telefono, model.Domicilio, model.Email,
            model.TotalReservas, model.FechaCreacion, usuario
        );
    }
}
