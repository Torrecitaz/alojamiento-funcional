using Microservicio.Cliente.DatAccess.Contexts;
using Microservicio.Cliente.DatAccess.Entities.Facturacion;
using Microservicio.Clientes.Business.DTOs.Facturacion;
using Microservicio.Clientes.Business.Interfaces;
using Microservicio.Clientes.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Clientes.Business.Services;

public class FacturaService : IFacturaService
{
    private readonly IFacturaRepository _facturaRepo;
    private readonly FacturacionDbContext _db;

    public FacturaService(IFacturaRepository facturaRepo, FacturacionDbContext db)
    {
        _facturaRepo = facturaRepo;
        _db = db;
    }

    public async Task<IEnumerable<FacturaResponse>> GetAllAsync()
    {
        var items = await _db.Facturas
            .Include(f => f.MetodoPago)
            .Include(f => f.Detalles)
            .ToListAsync();
        return items.Select(MapFactura);
    }

    public async Task<FacturaResponse?> GetByIdAsync(int id)
    {
        var f = await _db.Facturas
            .Include(f => f.MetodoPago)
            .Include(f => f.Detalles)
            .FirstOrDefaultAsync(f => f.FacturaId == id);
        return f == null ? null : MapFactura(f);
    }

    public async Task<IEnumerable<FacturaResponse>> GetByReservaIdAsync(int reservaId)
    {
        var items = await _facturaRepo.GetByReservaIdAsync(reservaId);
        return items.Select(MapFactura);
    }

    /// <summary>
    /// Usa el SP sp_registrar_factura_completa para crear la factura y su detalle atómicamente.
    /// </summary>
    public async Task CrearFacturaAsync(CrearFacturaRequest request)
    {
        await _facturaRepo.RegistrarFacturaCompletaSPAsync(
            request.ReservaId, request.MetodoPagoId, request.Monto, request.Descripcion);
    }

    public async Task<IEnumerable<MetodoPagoResponse>> GetMetodosPagoAsync()
    {
        var items = await _db.MetodosPago.ToListAsync();
        return items.Select(m => new MetodoPagoResponse(m.MetodoPagoId, m.Tipo));
    }

    private static FacturaResponse MapFactura(FacturaEntity f) => new(
        f.FacturaId, f.ReservaId, f.MetodoPagoId, f.Monto, f.Estado,
        f.FechaPago, f.FechaCreacion,
        f.MetodoPago?.Tipo,
        f.Detalles?.Select(d => new DetalleFacturaResponse(
            d.DetalleFacturaId, d.Descripcion, d.Cantidad, d.PrecioUnitario
        )).ToList()
    );
}
