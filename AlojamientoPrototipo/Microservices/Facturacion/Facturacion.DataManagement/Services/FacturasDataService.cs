using Facturacion.DataAccess.Entities;
using Facturacion.DataAccess.Repositories.Interfaces;
using Facturacion.DataManagement.Interfaces;
using Facturacion.DataManagement.Mappers;
using Facturacion.DataManagement.Models;

namespace Facturacion.DataManagement.Services;

public class FacturasDataService : IFacturasDataService
{
    private readonly IFacturasRepository _repo;

    public FacturasDataService(IFacturasRepository repo) => _repo = repo;

    public async Task<FacturaDataModel?> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity != null ? FacturacionMapper.ToDataModel(entity) : null;
    }

    public async Task<IEnumerable<FacturaDataModel>> GetByReservaIdAsync(int reservaId)
    {
        var entities = await _repo.FindAsync(f => f.ReservaId == reservaId);
        return entities.Select(FacturacionMapper.ToDataModel);
    }

    public async Task<FacturaDataModel> CreateAsync(FacturaDataModel model)
    {
        var entity = new FacturaEntity
        {
            ReservaId = model.ReservaId,
            MetodoPagoId = model.MetodoPagoId,
            Monto = model.Monto,
            Estado = model.Estado,
            FechaPago = model.FechaPago
        };

        foreach (var det in model.Detalles)
        {
            entity.Detalles.Add(new DetalleFacturaEntity
            {
                Descripcion = det.Descripcion,
                Cantidad = det.Cantidad,
                PrecioUnitario = det.PrecioUnitario
            });
        }

        var created = await _repo.AddAsync(entity);
        return FacturacionMapper.ToDataModel(created);
    }

    public async Task UpdateStatusAsync(int id, string nuevoEstado)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Factura {id} no encontrada");

        entity.Estado = nuevoEstado;
        entity.FechaModificacion = DateTime.UtcNow;
        if (nuevoEstado == "Pagado")
        {
            entity.FechaPago = DateTime.UtcNow;
        }
        await _repo.UpdateAsync(entity);
    }
}
