using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Alojamientos.DataAccess.Entities;

namespace Alojamientos.DataAccess.Configurations;

public class HabitacionesConfiguration : IEntityTypeConfiguration<HabitacionEntity>
{
    public void Configure(EntityTypeBuilder<HabitacionEntity> builder)
    {
        builder.ToTable("habitaciones");
        builder.HasKey(h => h.HabitacionId);

        builder.Property(h => h.Nombre).HasMaxLength(100).IsRequired();
        builder.Property(h => h.Descripcion).HasMaxLength(500);
        builder.Property(h => h.CapacidadAdultos).HasDefaultValue(2);
        builder.Property(h => h.CapacidadNinos).HasDefaultValue(0);
        builder.Property(h => h.NumBanos).HasDefaultValue(1);
        builder.Property(h => h.NumDormitorios).HasDefaultValue(1);
        builder.Property(h => h.TieneCocina).HasDefaultValue(false);
        builder.Property(h => h.TieneAireAcondicionado).HasDefaultValue(false);
        builder.Property(h => h.PrecioNoche).HasDefaultValue(0);

        // Relaciones
        builder.HasOne(h => h.Alojamiento)
               .WithMany(a => a.Habitaciones)
               .HasForeignKey(h => h.AlojamientoId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
