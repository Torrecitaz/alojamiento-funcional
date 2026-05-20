using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Facturacion.DataAccess.Entities;

namespace Facturacion.DataAccess.Configurations;

public class FacturaConfiguration : IEntityTypeConfiguration<FacturaEntity>
{
    public void Configure(EntityTypeBuilder<FacturaEntity> builder)
    {
        builder.ToTable("facturas");
        builder.HasKey(f => f.FacturaId);

        builder.Property(f => f.Monto).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(f => f.Estado).HasMaxLength(30).HasDefaultValue("Aprobado");
        builder.Property(f => f.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relaciones
        builder.HasOne(f => f.MetodoPago)
               .WithMany(m => m.Facturas)
               .HasForeignKey(f => f.MetodoPagoId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(f => f.Detalles)
               .WithOne(d => d.Factura)
               .HasForeignKey(d => d.FacturaId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
