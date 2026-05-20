using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Facturacion.DataAccess.Entities;

namespace Facturacion.DataAccess.Configurations;

public class AuditoriaGeneralConfiguration : IEntityTypeConfiguration<AuditoriaGeneralEntity>
{
    public void Configure(EntityTypeBuilder<AuditoriaGeneralEntity> builder)
    {
        builder.ToTable("auditoriageneral");
        builder.HasKey(a => a.AuditoriaId);

        builder.Property(a => a.NombreTabla).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Operacion).HasMaxLength(10).IsRequired();
        builder.Property(a => a.RegistroId).HasMaxLength(50).IsRequired();
        builder.Property(a => a.UsuarioAccion).HasMaxLength(100);
        builder.Property(a => a.Origen).HasMaxLength(45);
        builder.Property(a => a.FechaAccion).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
