using Microservicio.Clientes.Business.DTOs.Reservas;

namespace Microservicio.Clientes.Business.Interfaces;

public interface IReservaService
{
    Task<IEnumerable<ReservaResponse>> GetAllAsync();
    Task<ReservaResponse?> GetByIdAsync(int id);
    Task<ReservaResponse?> GetByCodigoAsync(string codigo);
    Task<IEnumerable<ReservaResponse>> GetByClienteIdAsync(int clienteId);
    /// <summary>
    /// Crea una reserva completa con ORQUESTACIÓN cross-DB:
    /// 1. Valida disponibilidad en DB_Alojamientos
    /// 2. Calcula noches con fn_calcular_noches de DB_Reservas
    /// 3. Inserta la reserva y detalles en DB_Reservas
    /// 4. Asigna código con sp_asignar_codigo_reserva
    /// 5. Bloquea fechas en CalendarioDisponibilidad de DB_Alojamientos
    /// </summary>
    Task<ReservaResponse> CrearReservaAsync(CrearReservaRequest request);
}
