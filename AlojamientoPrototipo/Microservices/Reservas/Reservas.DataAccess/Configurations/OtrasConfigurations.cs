using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservas.DataAccess.Entities;

namespace Reservas.DataAccess.Configurations;

public class ReservaDetalleHabitacionConfiguration : IEntityTypeConfiguration<ReservaDetalleHabitacionEntity>
{
    public void Configure(EntityTypeBuilder<ReservaDetalleHabitacionEntity> builder)
    {
        builder.ToTable("reservadetallehabitacion");
        builder.HasKey(r => r.DetalleId);

        builder.Property(r => r.PrecioPorNoche).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(r => r.NumNoches).IsRequired();
        builder.Property(r => r.SubTotalHabitacion).HasColumnType("decimal(12,2)").IsRequired();
    }
}

public class DescuentoConfiguration : IEntityTypeConfiguration<DescuentoEntity>
{
    public void Configure(EntityTypeBuilder<DescuentoEntity> builder)
    {
        builder.ToTable("descuentos");
        builder.HasKey(d => d.DescuentoId);

        builder.Property(d => d.Codigo).HasMaxLength(20).IsRequired();
        builder.HasIndex(d => d.Codigo).IsUnique();
        
        builder.Property(d => d.Porcentaje).HasColumnType("decimal(5,2)").IsRequired();
        builder.Property(d => d.Activo).HasDefaultValue(true);
    }
}
