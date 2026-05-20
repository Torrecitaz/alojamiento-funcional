using Microsoft.EntityFrameworkCore;
using Reservas.DataAccess.Configurations;
using Reservas.DataAccess.Entities;

namespace Reservas.DataAccess.Contexts;

public class ReservasDbContext : DbContext
{
    public ReservasDbContext(DbContextOptions<ReservasDbContext> options) : base(options) { }

    public DbSet<ReservaEntity> Reservas { get; set; } = null!;
    public DbSet<DescuentoEntity> Descuentos { get; set; } = null!;
    public DbSet<ReservaDetalleHabitacionEntity> DetallesHabitacion { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ReservaConfiguration());
        modelBuilder.ApplyConfiguration(new DescuentoConfiguration());
        modelBuilder.ApplyConfiguration(new ReservaDetalleHabitacionConfiguration());
    }
}
