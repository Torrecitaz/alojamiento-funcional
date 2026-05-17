using Microservicio.Cliente.DatAccess.Entities.Alojamientos;

namespace Microservicio.Clientes.DataManagement.Interfaces;

public interface IHabitacionRepository : IRepository<HabitacionEntity>
{
    Task<IEnumerable<HabitacionEntity>> GetByAlojamientoIdAsync(int alojamientoId);
}
