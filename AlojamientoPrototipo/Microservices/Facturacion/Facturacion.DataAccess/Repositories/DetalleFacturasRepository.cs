using Facturacion.DataAccess.Common;
using Facturacion.DataAccess.Contexts;
using Facturacion.DataAccess.Entities;
using Facturacion.DataAccess.Repositories.Interfaces;

namespace Facturacion.DataAccess.Repositories;

public class DetalleFacturasRepository : RepositoryBase<DetalleFacturaEntity>, IDetalleFacturasRepository
{
    public DetalleFacturasRepository(FacturacionDbContext context) : base(context) { }
}
