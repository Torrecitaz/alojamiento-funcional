using Microservicio.Cliente.DatAccess.Entities.Facturacion;

namespace Microservicio.Clientes.DataManagement.Interfaces;

public interface IFacturaRepository : IRepository<FacturaEntity>
{
    Task<IEnumerable<FacturaEntity>> GetByReservaIdAsync(int reservaId);
    /// <summary>
    /// Llama al SP sp_registrar_factura_completa de la base DB_Facturacion.
    /// Crea la Factura y su Detalle en una sola transacción atómica.
    /// </summary>
    Task RegistrarFacturaCompletaSPAsync(int reservaId, int metodoPagoId, decimal monto, string descripcion);
}
