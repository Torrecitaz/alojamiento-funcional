using Microservicio.Clientes.Business.DTOs.Facturacion;

namespace Microservicio.Clientes.Business.Interfaces;

public interface IFacturaService
{
    Task<IEnumerable<FacturaResponse>> GetAllAsync();
    Task<FacturaResponse?> GetByIdAsync(int id);
    Task<IEnumerable<FacturaResponse>> GetByReservaIdAsync(int reservaId);
    /// <summary>
    /// Usa el SP sp_registrar_factura_completa para crear Factura + Detalle atómicamente.
    /// </summary>
    Task CrearFacturaAsync(CrearFacturaRequest request);
    Task<IEnumerable<MetodoPagoResponse>> GetMetodosPagoAsync();
}
