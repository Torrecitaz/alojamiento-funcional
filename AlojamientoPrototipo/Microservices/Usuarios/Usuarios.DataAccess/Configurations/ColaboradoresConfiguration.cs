using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Usuarios.DataAccess.Entities;

namespace Usuarios.DataAccess.Configurations;

public class ColaboradoresConfiguration : IEntityTypeConfiguration<ColaboradorEntity>
{
    public void Configure(EntityTypeBuilder<ColaboradorEntity> builder)
    {
        builder.ToTable("colaboradores");
        builder.HasKey(c => c.ColaboradorId);

        builder.Property(c => c.NombreEmpresa).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Telefono).HasMaxLength(50);
        builder.Property(c => c.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
