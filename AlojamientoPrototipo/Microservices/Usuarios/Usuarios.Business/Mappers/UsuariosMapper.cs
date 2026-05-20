using Usuarios.Business.DTOs.Usuarios;
using Usuarios.DataManagement.Models;

namespace Usuarios.Business.Mappers;

public static class UsuariosBusinessMapper
{
    public static UsuarioResponse ToResponse(UsuarioDataModel model) => new(
        model.UsuarioId, model.Rol, model.Email,
        model.NombreCompleto, model.Estado, model.FechaCreacion
    );
}
