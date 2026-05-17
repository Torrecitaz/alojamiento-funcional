using Microservicio.Cliente.DatAccess.Entities.Reservas;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Cliente.DatAccess.Contexts;

public class ReservasDbContext : DbContext
{
    public ReservasDbContext(DbContextOptions<ReservasDbContext> options) : base(options) { }

    public DbSet<DescuentoEntity> Descuentos { get; set; } = null!;
    public DbSet<ReservaEntity> Reservas { get; set; } = null!;
    public DbSet<ReservaDetalleHabitacionEntity> ReservaDetalles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReservaEntity>()
            .HasOne(r => r.Descuento)
            .WithMany(d => d.Reservas)
            .HasForeignKey(r => r.DescuentoId);

        modelBuilder.Entity<ReservaDetalleHabitacionEntity>()
            .HasOne(rd => rd.Reserva)
            .WithMany(r => r.Detalles)
            .HasForeignKey(rd => rd.ReservaId);

        modelBuilder.Entity<ReservaEntity>()
            .HasIndex(r => r.CodigoReserva).IsUnique();

        modelBuilder.Entity<DescuentoEntity>()
            .HasIndex(d => d.Codigo).IsUnique();
    }
}
