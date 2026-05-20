using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Usuarios.DataAccess.Entities;

namespace Usuarios.DataAccess.Configurations;

public class ClientesConfiguration : IEntityTypeConfiguration<ClienteEntity>
{
    public void Configure(EntityTypeBuilder<ClienteEntity> builder)
    {
        builder.ToTable("clientes");
        builder.HasKey(c => c.ClienteId);

        builder.Property(c => c.Cedula).HasMaxLength(20).IsRequired();
        builder.Property(c => c.FotoUrl).HasMaxLength(500);
        builder.Property(c => c.Telefono).HasMaxLength(20);
        builder.Property(c => c.Domicilio).HasMaxLength(300);
        builder.Property(c => c.Email).HasMaxLength(200).IsRequired();
        builder.Property(c => c.TotalReservas).HasDefaultValue(0);
        builder.Property(c => c.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(c => c.Cedula).IsUnique();
        builder.HasIndex(c => c.Email).IsUnique();
    }
}
