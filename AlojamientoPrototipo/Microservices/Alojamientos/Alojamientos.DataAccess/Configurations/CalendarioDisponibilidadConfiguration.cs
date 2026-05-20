using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Alojamientos.DataAccess.Entities;

namespace Alojamientos.DataAccess.Configurations;

public class CalendarioDisponibilidadConfiguration : IEntityTypeConfiguration<CalendarioDisponibilidadEntity>
{
    public void Configure(EntityTypeBuilder<CalendarioDisponibilidadEntity> builder)
    {
        builder.ToTable("calendariodisponibilidad");
        builder.HasKey(c => c.CalendarioId);

        builder.Property(c => c.Fecha).IsRequired();
        builder.Property(c => c.Estado).HasMaxLength(20).HasDefaultValue("Disponible");

        // Relaciones
        builder.HasOne(c => c.Habitacion)
               .WithMany(h => h.Calendario)
               .HasForeignKey(c => c.HabitacionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
