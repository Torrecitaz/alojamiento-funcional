using Facturacion.Business.DTOs;

namespace Facturacion.Business.Interfaces;

public interface IMetodosPagoService
{
    Task<IEnumerable<MetodoPagoResponse>> GetAllAsync();
}
