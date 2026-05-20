using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Alojamientos.DataAccess.Entities;

namespace Alojamientos.DataAccess.Configurations;

public class TipoAlojamientoConfiguration : IEntityTypeConfiguration<TipoAlojamientoEntity>
{
    public void Configure(EntityTypeBuilder<TipoAlojamientoEntity> builder)
    {
        builder.ToTable("tiposalojamiento");
        builder.HasKey(t => t.TipoAlojamientoId);

        builder.Property(t => t.Nombre).HasMaxLength(50).IsRequired();
        builder.Property(t => t.Descripcion).HasMaxLength(200);
    }
}
