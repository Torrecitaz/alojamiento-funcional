using Usuarios.Business.DTOs.Colaboradores;
using Usuarios.DataManagement.Models;

namespace Usuarios.Business.Mappers;

public static class ColaboradoresBusinessMapper
{
    public static ColaboradorResponse ToResponse(ColaboradorDataModel model) => new(
        model.ColaboradorId,
        model.UsuarioId,
        model.NombreEmpresa,
        model.Telefono,
        model.FechaCreacion,
        model.Usuario?.Email ?? string.Empty,
        model.Usuario?.NombreCompleto ?? string.Empty
    );
}
