using Usuarios.DataAccess.Entities;
using Usuarios.DataManagement.Models;

namespace Usuarios.DataManagement.Mappers;

public static class LocalizacionesMapper
{
    public static LocalizacionDataModel ToDataModel(LocalizacionEntity entity) => new()
    {
        LocalizacionId = entity.LocalizacionId,
        Descripcion = entity.Descripcion,
        PoligonoWkt = entity.Poligono != null ? entity.Poligono.ToText() : null
    };
}
