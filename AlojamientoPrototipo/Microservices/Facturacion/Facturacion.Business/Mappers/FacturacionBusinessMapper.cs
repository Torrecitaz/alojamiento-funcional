using Facturacion.Business.DTOs;
using Facturacion.DataManagement.Models;

namespace Facturacion.Business.Mappers;

public static class FacturacionBusinessMapper
{
    public static FacturaResponse ToResponse(FacturaDataModel model) => new()
    {
        FacturaId = model.FacturaId,
        ReservaId = model.ReservaId,
        MetodoPagoId = model.MetodoPagoId,
        MetodoPagoTipo = model.MetodoPago?.Tipo,
        Monto = model.Monto,
        Estado = model.Estado,
        FechaPago = model.FechaPago,
        FechaCreacion = model.FechaCreacion,
        Detalles = model.Detalles.Select(ToResponse).ToList()
    };

    public static FacturaResumenResponse ToResumenResponse(FacturaDataModel model) => new()
    {
        FacturaId = model.FacturaId,
        ReservaId = model.ReservaId,
        MetodoPagoTipo = model.MetodoPago?.Tipo,
        Monto = model.Monto,
        Estado = model.Estado,
        FechaPago = model.FechaPago,
        FechaCreacion = model.FechaCreacion,
        TotalDetalles = model.Detalles.Count
    };

    public static DetalleFacturaResponse ToResponse(DetalleFacturaDataModel model) => new()
    {
        DetalleFacturaId = model.DetalleFacturaId,
        Descripcion = model.Descripcion,
        Cantidad = model.Cantidad,
        PrecioUnitario = model.PrecioUnitario
    };

    public static MetodoPagoResponse ToResponse(MetodoPagoClienteDataModel model) => new()
    {
        MetodoPagoId = model.MetodoPagoId,
        Tipo = model.Tipo
    };
}
