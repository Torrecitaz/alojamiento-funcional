using Microservicio.Cliente.DatAccess.Entities.Facturacion;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Cliente.DatAccess.Contexts;

public class FacturacionDbContext : DbContext
{
    public FacturacionDbContext(DbContextOptions<FacturacionDbContext> options) : base(options) { }

    public DbSet<MetodoPagoEntity> MetodosPago { get; set; } = null!;
    public DbSet<FacturaEntity> Facturas { get; set; } = null!;
    public DbSet<DetalleFacturaEntity> DetalleFacturas { get; set; } = null!;
    public DbSet<AuditoriaEntity> AuditoriaGeneral { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FacturaEntity>()
            .HasOne(f => f.MetodoPago)
            .WithMany(m => m.Facturas)
            .HasForeignKey(f => f.MetodoPagoId);

        modelBuilder.Entity<DetalleFacturaEntity>()
            .HasOne(d => d.Factura)
            .WithMany(f => f.Detalles)
            .HasForeignKey(d => d.FacturaId);
    }
}
