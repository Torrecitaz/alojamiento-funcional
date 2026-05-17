using Microservicio.Cliente.DatAccess.Entities.Reservas;

namespace Microservicio.Clientes.DataManagement.Interfaces;

public interface IReservaRepository : IRepository<ReservaEntity>
{
    Task<IEnumerable<ReservaEntity>> GetByClienteIdAsync(int clienteId);
    Task<ReservaEntity?> GetByCodigoAsync(string codigoReserva);
    Task<ReservaEntity?> GetWithDetallesAsync(int reservaId);
    /// <summary>
    /// Llama al SP sp_asignar_codigo_reserva de la base DB_Reservas.
    /// </summary>
    Task AsignarCodigoReservaSPAsync(int reservaId);
    /// <summary>
    /// Llama a la función fn_calcular_noches de la base DB_Reservas.
    /// </summary>
    Task<int> CalcularNochesFnAsync(DateOnly checkin, DateOnly checkout);
}
