using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Usuarios.DataAccess.Entities;

namespace Usuarios.DataAccess.Configurations;

public class UsuariosConfiguration : IEntityTypeConfiguration<UsuarioEntity>
{
    public void Configure(EntityTypeBuilder<UsuarioEntity> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(u => u.UsuarioId);

        builder.Property(u => u.Rol).HasMaxLength(10).HasDefaultValue("Cliente");
        builder.Property(u => u.Email).HasMaxLength(200).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(u => u.NombreCompleto).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Estado).HasDefaultValue(true);
        builder.Property(u => u.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(u => u.Email).IsUnique();

        // Relación 1:1 con Cliente
        builder.HasOne(u => u.Cliente)
               .WithOne(c => c.Usuario)
               .HasForeignKey<ClienteEntity>(c => c.UsuarioId);
    }
}
