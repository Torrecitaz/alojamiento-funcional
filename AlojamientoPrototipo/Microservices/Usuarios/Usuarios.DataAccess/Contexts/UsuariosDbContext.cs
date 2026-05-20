using Microsoft.EntityFrameworkCore;
using Usuarios.DataAccess.Configurations;
using Usuarios.DataAccess.Entities;

namespace Usuarios.DataAccess.Contexts;

public class UsuariosDbContext : DbContext
{
    public UsuariosDbContext(DbContextOptions<UsuariosDbContext> options) : base(options) { }

    public DbSet<LocalizacionEntity> Localizaciones { get; set; } = null!;
    public DbSet<UsuarioEntity> Usuarios { get; set; } = null!;
    public DbSet<ClienteEntity> Clientes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica las configuraciones desde archivos separados (Clean Architecture)
        modelBuilder.ApplyConfiguration(new UsuariosConfiguration());
        modelBuilder.ApplyConfiguration(new ClientesConfiguration());
        modelBuilder.ApplyConfiguration(new LocalizacionesConfiguration());
    }
}
