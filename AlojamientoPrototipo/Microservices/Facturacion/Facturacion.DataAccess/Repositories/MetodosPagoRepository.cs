using Facturacion.DataAccess.Common;
using Facturacion.DataAccess.Contexts;
using Facturacion.DataAccess.Entities;
using Facturacion.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.DataAccess.Repositories;

public class MetodosPagoRepository : RepositoryBase<MetodoPagoClienteEntity>, IMetodosPagoRepository
{
    public MetodosPagoRepository(FacturacionDbContext context) : base(context) { }

    public async Task<MetodoPagoClienteEntity?> GetByExternalIdAsync(Guid externalId)
        => await _dbSet.FirstOrDefaultAsync(m => m.ExternalId == externalId);
}
