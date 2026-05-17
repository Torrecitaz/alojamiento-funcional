using Microservicio.Cliente.DatAccess.Entities.Alojamientos;

namespace Microservicio.Clientes.DataManagement.Interfaces;

public interface ICalendarioRepository : IRepository<CalendarioDisponibilidadEntity>
{
    Task<IEnumerable<CalendarioDisponibilidadEntity>> GetByHabitacionAsync(int habitacionId, DateOnly desde, DateOnly hasta);
    Task<bool> IsDisponibleAsync(int habitacionId, DateOnly desde, DateOnly hasta);
    Task BloquearFechasAsync(int habitacionId, DateOnly desde, DateOnly hasta);
}
