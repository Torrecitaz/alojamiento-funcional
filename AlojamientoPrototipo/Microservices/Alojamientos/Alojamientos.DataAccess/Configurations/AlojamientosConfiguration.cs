using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Alojamientos.DataAccess.Entities;

namespace Alojamientos.DataAccess.Configurations;

public class AlojamientosConfiguration : IEntityTypeConfiguration<AlojamientoEntity>
{
    public void Configure(EntityTypeBuilder<AlojamientoEntity> builder)
    {
        builder.ToTable("alojamientos");
        builder.HasKey(a => a.AlojamientoId);

        builder.Property(a => a.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Ciudad).HasMaxLength(100);
        builder.Property(a => a.Direccion).HasMaxLength(300).IsRequired();
        builder.Property(a => a.Estado).HasMaxLength(20).HasDefaultValue("Pendiente");
        builder.Property(a => a.CalificacionPromedio).HasDefaultValue(0);
        builder.Property(a => a.TotalResenas).HasDefaultValue(0);
        builder.Property(a => a.AdmiteMascotas).HasDefaultValue(false);
        builder.Property(a => a.TienePiscina).HasDefaultValue(false);
        builder.Property(a => a.TieneParqueadero).HasDefaultValue(false);
        builder.Property(a => a.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relaciones
        builder.HasOne(a => a.TipoAlojamiento)
               .WithMany(t => t.Alojamientos)
               .HasForeignKey(a => a.TipoAlojamientoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
