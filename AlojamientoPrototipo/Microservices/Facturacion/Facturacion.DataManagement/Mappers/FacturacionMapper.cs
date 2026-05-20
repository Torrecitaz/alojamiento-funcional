using Facturacion.DataAccess.Entities;
using Facturacion.DataManagement.Models;

namespace Facturacion.DataManagement.Mappers;

public static class FacturacionMapper
{
    public static MetodoPagoClienteDataModel ToDataModel(MetodoPagoClienteEntity entity) => new()
    {
        MetodoPagoId = entity.MetodoPagoId,
        Tipo = entity.Tipo
    };

    public static DetalleFacturaDataModel ToDataModel(DetalleFacturaEntity entity) => new()
    {
        DetalleFacturaId = entity.DetalleFacturaId,
        FacturaId = entity.FacturaId,
        Descripcion = entity.Descripcion,
        Cantidad = entity.Cantidad,
        PrecioUnitario = entity.PrecioUnitario
    };

    public static FacturaDataModel ToDataModel(FacturaEntity entity) => new()
    {
        FacturaId = entity.FacturaId,
        ReservaId = entity.ReservaId,
        MetodoPagoId = entity.MetodoPagoId,
        Monto = entity.Monto,
        Estado = entity.Estado,
        FechaPago = entity.FechaPago,
        FechaCreacion = entity.FechaCreacion,
        FechaModificacion = entity.FechaModificacion,
        MetodoPago = entity.MetodoPago != null ? ToDataModel(entity.MetodoPago) : null,
        Detalles = entity.Detalles?.Select(ToDataModel).ToList() ?? new List<DetalleFacturaDataModel>()
    };

    public static AuditoriaGeneralDataModel ToDataModel(AuditoriaGeneralEntity entity) => new()
    {
        AuditoriaId = entity.AuditoriaId,
        NombreTabla = entity.NombreTabla,
        Operacion = entity.Operacion,
        RegistroId = entity.RegistroId,
        DatosAnteriores = entity.DatosAnteriores,
        DatosNuevos = entity.DatosNuevos,
        UsuarioAccion = entity.UsuarioAccion,
        FechaAccion = entity.FechaAccion,
        Origen = entity.Origen
    };
}
