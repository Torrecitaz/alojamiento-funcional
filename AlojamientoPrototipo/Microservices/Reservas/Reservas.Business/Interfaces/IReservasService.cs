using Reservas.Business.DTOs;

namespace Reservas.Business.Interfaces;

public interface IReservasService
{
    Task<ReservaResponse> GetByIdAsync(int id);
    Task<IEnumerable<ReservaResponse>> GetByClienteIdAsync(int clienteId);
    Task<IEnumerable<ReservaResumenResponse>> GetResumenByClienteIdAsync(int clienteId);
    Task<ReservaResponse> CrearAsync(CrearReservaRequest request);
    Task ActualizarEstadoAsync(int id, ActualizarEstadoReservaRequest request);
}
