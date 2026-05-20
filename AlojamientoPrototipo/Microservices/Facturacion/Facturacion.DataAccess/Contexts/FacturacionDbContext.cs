using Microsoft.EntityFrameworkCore;
using Facturacion.DataAccess.Configurations;
using Facturacion.DataAccess.Entities;

namespace Facturacion.DataAccess.Contexts;

public class FacturacionDbContext : DbContext
{
    public FacturacionDbContext(DbContextOptions<FacturacionDbContext> options) : base(options) { }

    public DbSet<MetodoPagoClienteEntity> MetodosPago { get; set; } = null!;
    public DbSet<FacturaEntity> Facturas { get; set; } = null!;
    public DbSet<DetalleFacturaEntity> DetallesFactura { get; set; } = null!;
    public DbSet<AuditoriaGeneralEntity> Auditoria { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new MetodoPagoClienteConfiguration());
        modelBuilder.ApplyConfiguration(new FacturaConfiguration());
        modelBuilder.ApplyConfiguration(new DetalleFacturaConfiguration());
        modelBuilder.ApplyConfiguration(new AuditoriaGeneralConfiguration());
    }
}
