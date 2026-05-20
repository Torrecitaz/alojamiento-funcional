using Facturacion.DataAccess.Common;
using Facturacion.DataAccess.Contexts;
using Facturacion.DataAccess.Entities;
using Facturacion.DataAccess.Repositories.Interfaces;

namespace Facturacion.DataAccess.Repositories;

public class FacturasRepository : RepositoryBase<FacturaEntity>, IFacturasRepository
{
    public FacturasRepository(FacturacionDbContext context) : base(context) { }
}
