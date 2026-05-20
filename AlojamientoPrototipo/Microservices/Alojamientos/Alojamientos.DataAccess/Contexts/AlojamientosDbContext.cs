using Microsoft.EntityFrameworkCore;
using Alojamientos.DataAccess.Configurations;
using Alojamientos.DataAccess.Entities;

namespace Alojamientos.DataAccess.Contexts;

public class AlojamientosDbContext : DbContext
{
    public AlojamientosDbContext(DbContextOptions<AlojamientosDbContext> options) : base(options) { }

    public DbSet<AlojamientoEntity> Alojamientos { get; set; } = null!;
    public DbSet<HabitacionEntity> Habitaciones { get; set; } = null!;
    public DbSet<TipoAlojamientoEntity> TiposAlojamiento { get; set; } = null!;
    public DbSet<AlojamientoFotoEntity> AlojamientoFotos { get; set; } = null!;
    public DbSet<CalendarioDisponibilidadEntity> CalendarioDisponibilidad { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AlojamientosConfiguration());
        modelBuilder.ApplyConfiguration(new HabitacionesConfiguration());
        modelBuilder.ApplyConfiguration(new TipoAlojamientoConfiguration());
        modelBuilder.ApplyConfiguration(new AlojamientoFotoConfiguration());
        modelBuilder.ApplyConfiguration(new CalendarioDisponibilidadConfiguration());
    }
}
