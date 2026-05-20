using Facturacion.Business.DTOs;

namespace Facturacion.Business.Interfaces;

public interface IFacturasService
{
    Task<FacturaResponse> GetByIdAsync(int id);
    Task<IEnumerable<FacturaResponse>> GetByReservaIdAsync(int reservaId);
    Task<IEnumerable<FacturaResumenResponse>> GetResumenByReservaIdAsync(int reservaId);
    Task<FacturaResponse> CrearAsync(CrearFacturaRequest request);
    Task AprobarFacturaAsync(int id);
    Task RechazarFacturaAsync(int id);
}
