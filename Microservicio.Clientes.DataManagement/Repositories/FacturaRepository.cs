using Microservicio.Cliente.DatAccess.Contexts;
using Microservicio.Cliente.DatAccess.Entities.Facturacion;
using Microservicio.Clientes.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Clientes.DataManagement.Repositories;

public class FacturaRepository : Repository<FacturaEntity>, IFacturaRepository
{
    private readonly FacturacionDbContext _db;

    public FacturaRepository(FacturacionDbContext context) : base(context)
    {
        _db = context;
    }

    public async Task<IEnumerable<FacturaEntity>> GetByReservaIdAsync(int reservaId)
        => await _db.Facturas
            .Include(f => f.MetodoPago)
            .Include(f => f.Detalles)
            .Where(f => f.ReservaId == reservaId)
            .ToListAsync();

    /// <summary>
    /// Ejecuta el SP sp_registrar_factura_completa de la base DB_Facturacion.
    /// Crea la Factura y su DetalleFact en una sola transacción atómica.
    /// </summary>
    public async Task RegistrarFacturaCompletaSPAsync(int reservaId, int metodoPagoId, decimal monto, string descripcion)
    {
        await _db.Database.ExecuteSqlRawAsync(
            "CALL sp_registrar_factura_completa({0}, {1}, {2}, {3})",
            reservaId, metodoPagoId, monto, descripcion);
    }
}
