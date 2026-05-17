using Microservicio.Cliente.DatAccess.Contexts;
using Microservicio.Cliente.DatAccess.Entities.Alojamientos;
using Microservicio.Clientes.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Clientes.DataManagement.Repositories;

public class CalendarioRepository : Repository<CalendarioDisponibilidadEntity>, ICalendarioRepository
{
    private readonly AlojamientosDbContext _db;

    public CalendarioRepository(AlojamientosDbContext context) : base(context)
    {
        _db = context;
    }

    public async Task<IEnumerable<CalendarioDisponibilidadEntity>> GetByHabitacionAsync(int habitacionId, DateOnly desde, DateOnly hasta)
        => await _db.CalendarioDisponibilidad
            .Where(c => c.HabitacionId == habitacionId && c.Fecha >= desde && c.Fecha <= hasta)
            .OrderBy(c => c.Fecha)
            .ToListAsync();

    /// <summary>
    /// Verifica si una habitación está disponible en TODAS las fechas del rango.
    /// Retorna true si NO hay ninguna entrada con estado "Ocupado" o "Bloqueado".
    /// </summary>
    public async Task<bool> IsDisponibleAsync(int habitacionId, DateOnly desde, DateOnly hasta)
    {
        var ocupadas = await _db.CalendarioDisponibilidad
            .Where(c => c.HabitacionId == habitacionId
                     && c.Fecha >= desde && c.Fecha < hasta
                     && c.Estado != "Disponible")
            .CountAsync();

        return ocupadas == 0;
    }

    /// <summary>
    /// Inserta las fechas del rango como "Ocupado" en el calendario.
    /// Esto se ejecuta cuando una reserva se confirma.
    /// </summary>
    public async Task BloquearFechasAsync(int habitacionId, DateOnly desde, DateOnly hasta)
    {
        var fechas = new List<CalendarioDisponibilidadEntity>();
        for (var fecha = desde; fecha < hasta; fecha = fecha.AddDays(1))
        {
            fechas.Add(new CalendarioDisponibilidadEntity
            {
                HabitacionId = habitacionId,
                Fecha = fecha,
                Estado = "Ocupado"
            });
        }

        await _db.CalendarioDisponibilidad.AddRangeAsync(fechas);
        await _db.SaveChangesAsync();
    }
}
