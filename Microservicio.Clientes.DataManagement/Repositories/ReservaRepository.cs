using Microservicio.Cliente.DatAccess.Contexts;
using Microservicio.Cliente.DatAccess.Entities.Reservas;
using Microservicio.Clientes.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Clientes.DataManagement.Repositories;

public class ReservaRepository : Repository<ReservaEntity>, IReservaRepository
{
    private readonly ReservasDbContext _db;

    public ReservaRepository(ReservasDbContext context) : base(context)
    {
        _db = context;
    }

    public async Task<IEnumerable<ReservaEntity>> GetByClienteIdAsync(int clienteId)
        => await _db.Reservas
            .Include(r => r.Detalles)
            .Where(r => r.ClienteId == clienteId)
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync();

    public async Task<ReservaEntity?> GetByCodigoAsync(string codigoReserva)
        => await _db.Reservas
            .Include(r => r.Detalles)
            .FirstOrDefaultAsync(r => r.CodigoReserva == codigoReserva);

    public async Task<ReservaEntity?> GetWithDetallesAsync(int reservaId)
        => await _db.Reservas
            .Include(r => r.Detalles)
            .Include(r => r.Descuento)
            .FirstOrDefaultAsync(r => r.ReservaId == reservaId);

    /// <summary>
    /// Ejecuta el SP sp_asignar_codigo_reserva de la base DB_Reservas.
    /// Genera un código único para la reserva recién creada.
    /// </summary>
    public async Task AsignarCodigoReservaSPAsync(int reservaId)
    {
        await _db.Database.ExecuteSqlRawAsync(
            "CALL sp_asignar_codigo_reserva({0})", reservaId);
    }

    /// <summary>
    /// Ejecuta la función fn_calcular_noches de la base DB_Reservas.
    /// Retorna la cantidad de noches entre dos fechas.
    /// </summary>
    public async Task<int> CalcularNochesFnAsync(DateOnly checkin, DateOnly checkout)
    {
        var result = await _db.Database
            .SqlQueryRaw<int>("SELECT fn_calcular_noches({0}::date, {1}::date) AS \"Value\"",
                checkin.ToString("yyyy-MM-dd"), checkout.ToString("yyyy-MM-dd"))
            .FirstOrDefaultAsync();

        return result;
    }
}
