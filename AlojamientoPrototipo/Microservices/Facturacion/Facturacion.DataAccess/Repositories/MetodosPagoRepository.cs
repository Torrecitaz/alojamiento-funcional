using Facturacion.DataAccess.Common;
using Facturacion.DataAccess.Contexts;
using Facturacion.DataAccess.Entities;
using Facturacion.DataAccess.Repositories.Interfaces;

namespace Facturacion.DataAccess.Repositories;

public class MetodosPagoRepository : RepositoryBase<MetodoPagoClienteEntity>, IMetodosPagoRepository
{
    public MetodosPagoRepository(FacturacionDbContext context) : base(context) { }
}
