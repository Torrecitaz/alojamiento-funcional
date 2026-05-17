using Microservicio.Cliente.DatAccess.Contexts;
using Microservicio.Cliente.DatAccess.Entities.Alojamientos;
using Microservicio.Clientes.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Clientes.DataManagement.Repositories;

public class AlojamientoRepository : Repository<AlojamientoEntity>, IAlojamientoRepository
{
    private readonly AlojamientosDbContext _db;

    public AlojamientoRepository(AlojamientosDbContext context) : base(context)
    {
        _db = context;
    }

    public async Task<IEnumerable<AlojamientoEntity>> GetByCiudadAsync(string ciudad)
        => await _db.Alojamientos
            .Include(a => a.TipoAlojamiento)
            .Include(a => a.Fotos)
            .Where(a => a.Ciudad != null && a.Ciudad.ToLower().Contains(ciudad.ToLower()))
            .ToListAsync();

    public async Task<AlojamientoEntity?> GetWithHabitacionesAsync(int alojamientoId)
        => await _db.Alojamientos
            .Include(a => a.TipoAlojamiento)
            .Include(a => a.Fotos)
            .Include(a => a.Habitaciones)
            .FirstOrDefaultAsync(a => a.AlojamientoId == alojamientoId);
}
