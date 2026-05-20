using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Alojamientos.DataAccess.Entities;

namespace Alojamientos.DataAccess.Configurations;

public class AlojamientoFotoConfiguration : IEntityTypeConfiguration<AlojamientoFotoEntity>
{
    public void Configure(EntityTypeBuilder<AlojamientoFotoEntity> builder)
    {
        builder.ToTable("alojamientofotos");
        builder.HasKey(f => f.FotoId);

        builder.Property(f => f.Url).HasMaxLength(500).IsRequired();
        builder.Property(f => f.Descripcion).HasMaxLength(200);
        builder.Property(f => f.Orden).HasDefaultValue(0);

        // Relaciones
        builder.HasOne(f => f.Alojamiento)
               .WithMany(a => a.Fotos)
               .HasForeignKey(f => f.AlojamientoId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
