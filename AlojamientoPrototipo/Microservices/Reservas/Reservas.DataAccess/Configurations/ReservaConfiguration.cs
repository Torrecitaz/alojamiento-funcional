using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservas.DataAccess.Entities;

namespace Reservas.DataAccess.Configurations;

public class ReservaConfiguration : IEntityTypeConfiguration<ReservaEntity>
{
    public void Configure(EntityTypeBuilder<ReservaEntity> builder)
    {
        builder.ToTable("reservas");
        builder.HasKey(r => r.ReservaId);

        builder.Property(r => r.FechaCheckIn).IsRequired();
        builder.Property(r => r.FechaCheckOut).IsRequired();
        builder.Property(r => r.NumAdultos).HasDefaultValue(1);
        builder.Property(r => r.NumNinos).HasDefaultValue(0);
        builder.Property(r => r.LlevaMascotas).HasDefaultValue(false);
        builder.Property(r => r.NumHabitaciones).HasDefaultValue(1);
        builder.Property(r => r.SubTotal).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(r => r.Total).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(r => r.Estado).HasMaxLength(30).HasDefaultValue("Pendiente");
        builder.Property(r => r.CodigoReserva).HasMaxLength(20).IsRequired();
        builder.Property(r => r.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relaciones
        builder.HasOne(r => r.Descuento)
               .WithMany(d => d.Reservas)
               .HasForeignKey(r => r.DescuentoId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.DetallesHabitacion)
               .WithOne(d => d.Reserva)
               .HasForeignKey(d => d.ReservaId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
