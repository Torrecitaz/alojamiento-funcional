using Facturacion.DataAccess.Common;
using Facturacion.DataAccess.Contexts;
using Facturacion.DataAccess.Entities;
using Facturacion.DataAccess.Repositories.Interfaces;

namespace Facturacion.DataAccess.Repositories;

public class AuditoriaRepository : RepositoryBase<AuditoriaGeneralEntity>, IAuditoriaRepository
{
    public AuditoriaRepository(FacturacionDbContext context) : base(context) { }
}
