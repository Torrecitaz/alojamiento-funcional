using Microservicio.Cliente.DatAccess.Entities.Alojamientos;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Cliente.DatAccess.Contexts;

public class AlojamientosDbContext : DbContext
{
    public AlojamientosDbContext(DbContextOptions<AlojamientosDbContext> options) : base(options) { }

    public DbSet<TipoAlojamientoEntity> TiposAlojamiento { get; set; } = null!;
    public DbSet<AlojamientoEntity> Alojamientos { get; set; } = null!;
    public DbSet<AlojamientoFotoEntity> AlojamientoFotos { get; set; } = null!;
    public DbSet<HabitacionEntity> Habitaciones { get; set; } = null!;
    public DbSet<CalendarioDisponibilidadEntity> CalendarioDisponibilidad { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AlojamientoEntity>()
            .HasOne(a => a.TipoAlojamiento)
            .WithMany(t => t.Alojamientos)
            .HasForeignKey(a => a.TipoAlojamientoId);

        modelBuilder.Entity<AlojamientoFotoEntity>()
            .HasOne(f => f.Alojamiento)
            .WithMany(a => a.Fotos)
            .HasForeignKey(f => f.AlojamientoId);

        modelBuilder.Entity<HabitacionEntity>()
            .HasOne(h => h.Alojamiento)
            .WithMany(a => a.Habitaciones)
            .HasForeignKey(h => h.AlojamientoId);

        modelBuilder.Entity<CalendarioDisponibilidadEntity>()
            .HasOne(c => c.Habitacion)
            .WithMany(h => h.Calendario)
            .HasForeignKey(c => c.HabitacionId);

        // Índice único: una habitación solo puede tener una entrada por fecha
        modelBuilder.Entity<CalendarioDisponibilidadEntity>()
            .HasIndex(c => new { c.HabitacionId, c.Fecha }).IsUnique();
    }
}
