using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Facturacion.DataAccess.Entities;

namespace Facturacion.DataAccess.Configurations;

public class MetodoPagoClienteConfiguration : IEntityTypeConfiguration<MetodoPagoClienteEntity>
{
    public void Configure(EntityTypeBuilder<MetodoPagoClienteEntity> builder)
    {
        builder.ToTable("metodospagocliente");
        builder.HasKey(m => m.MetodoPagoId);

        builder.Property(m => m.Tipo).HasMaxLength(30).IsRequired();
    }
}
