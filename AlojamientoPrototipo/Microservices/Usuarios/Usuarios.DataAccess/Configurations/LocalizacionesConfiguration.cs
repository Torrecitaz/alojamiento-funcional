using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Usuarios.DataAccess.Entities;

namespace Usuarios.DataAccess.Configurations;

public class LocalizacionesConfiguration : IEntityTypeConfiguration<LocalizacionEntity>
{
    public void Configure(EntityTypeBuilder<LocalizacionEntity> builder)
    {
        builder.ToTable("localizaciones");
        builder.HasKey(l => l.LocalizacionId);

        builder.Property(l => l.Descripcion).HasMaxLength(500);
    }
}
