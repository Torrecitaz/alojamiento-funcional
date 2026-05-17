using Microservicio.Cliente.DatAccess.Contexts;
using Microservicio.Cliente.DatAccess.Entities.Alojamientos;
using Microservicio.Clientes.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Clientes.DataManagement.Repositories;

public class HabitacionRepository : Repository<HabitacionEntity>, IHabitacionRepository
{
    private readonly AlojamientosDbContext _db;

    public HabitacionRepository(AlojamientosDbContext context) : base(context)
    {
        _db = context;
    }

    public async Task<IEnumerable<HabitacionEntity>> GetByAlojamientoIdAsync(int alojamientoId)
        => await _db.Habitaciones
            .Where(h => h.AlojamientoId == alojamientoId)
            .ToListAsync();
}
