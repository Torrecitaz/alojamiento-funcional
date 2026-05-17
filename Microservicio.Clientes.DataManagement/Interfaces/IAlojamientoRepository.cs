using Microservicio.Cliente.DatAccess.Entities.Alojamientos;

namespace Microservicio.Clientes.DataManagement.Interfaces;

public interface IAlojamientoRepository : IRepository<AlojamientoEntity>
{
    Task<IEnumerable<AlojamientoEntity>> GetByCiudadAsync(string ciudad);
    Task<AlojamientoEntity?> GetWithHabitacionesAsync(int alojamientoId);
}
