using Microservicio.Cliente.DatAccess.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddDbContext<BookingDbContext>(opt =>
    opt.UseSqlServer(@"Server=(localdb)\matitas;Database=BookingDB;Trusted_Connection=True;TrustServerCertificate=True;"));

var sp = services.BuildServiceProvider();
using var scope = sp.CreateScope();
var ctx = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
var usuario = await ctx.Usuarios.FirstOrDefaultAsync(u => u.Email == "admin@bookingapp.com");
Console.WriteLine(usuario == null ? "USUARIO NO ENCONTRADO" : $"OK: {usuario.NombreCompleto}");
