using Microservicio.Cliente.DatAccess.Entities.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Cliente.DatAccess.Contexts;

public class UsuariosDbContext : DbContext
{
    public UsuariosDbContext(DbContextOptions<UsuariosDbContext> options) : base(options) { }

    public DbSet<LocalizacionEntity> Localizaciones { get; set; } = null!;
    public DbSet<UsuarioEntity> Usuarios { get; set; } = null!;
    public DbSet<ClienteEntity> Clientes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relación 1:1 Usuario <-> Cliente
        modelBuilder.Entity<ClienteEntity>()
            .HasOne(c => c.Usuario)
            .WithOne(u => u.Cliente)
            .HasForeignKey<ClienteEntity>(c => c.UsuarioId);

        modelBuilder.Entity<ClienteEntity>()
            .HasIndex(c => c.Cedula).IsUnique();

        modelBuilder.Entity<ClienteEntity>()
            .HasIndex(c => c.Email).IsUnique();

        modelBuilder.Entity<UsuarioEntity>()
            .HasIndex(u => u.Email).IsUnique();
    }
}
