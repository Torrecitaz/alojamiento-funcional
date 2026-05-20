using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Facturacion.DataAccess.Entities;

namespace Facturacion.DataAccess.Configurations;

public class DetalleFacturaConfiguration : IEntityTypeConfiguration<DetalleFacturaEntity>
{
    public void Configure(EntityTypeBuilder<DetalleFacturaEntity> builder)
    {
        builder.ToTable("detallefacturas");
        builder.HasKey(d => d.DetalleFacturaId);

        builder.Property(d => d.Descripcion).HasMaxLength(200).IsRequired();
        builder.Property(d => d.Cantidad).HasDefaultValue(1);
        builder.Property(d => d.PrecioUnitario).HasColumnType("decimal(12,2)").IsRequired();
    }
}
