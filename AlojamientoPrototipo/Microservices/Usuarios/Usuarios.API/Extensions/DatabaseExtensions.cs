using Microsoft.EntityFrameworkCore;
using Usuarios.DataAccess.Contexts;
using Usuarios.DataAccess.Entities;

namespace Usuarios.API.Extensions;

public static class DatabaseExtensions
{
    public static void SeedDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsuariosDbContext>();

        try
        {
            // Ejecutar migraciones automáticas si es necesario
            context.Database.EnsureCreated();

            // Verificar si el administrador ya existe
            var adminEmail = "admin@alojaexpress.com";
            var adminUser = context.Usuarios.FirstOrDefault(u => u.Email == adminEmail);

            if (adminUser == null)
            {
                Console.WriteLine($"[SEED] Sembrando administrador default: {adminEmail}...");
                
                // Generar hash de contraseña usando BCrypt
                var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");

                var newAdmin = new UsuarioEntity
                {
                    Rol = "Administrador",
                    Email = adminEmail,
                    PasswordHash = passwordHash,
                    NombreCompleto = "Administrador AlojaExpress",
                    Estado = true,
                    FechaCreacion = DateTime.UtcNow
                };

                context.Usuarios.Add(newAdmin);
                context.SaveChanges();
                Console.WriteLine("[SEED] Administrador sembrado correctamente.");
            }
            else
            {
                // Asegurar que tenga el rol de Administrador
                if (adminUser.Rol != "Administrador")
                {
                    Console.WriteLine("[SEED] Actualizando rol de administrador existente a 'Administrador'...");
                    adminUser.Rol = "Administrador";
                    context.SaveChanges();
                }
            }

            // Asegurar que daniel@gmail.com tenga la contraseña daniel2005 con hash BCrypt
            var danielEmail = "daniel@gmail.com";
            var danielUser = context.Usuarios.FirstOrDefault(u => u.Email == danielEmail);
            if (danielUser != null)
            {
                Console.WriteLine("[SEED] Actualizando contraseña de daniel@gmail.com...");
                danielUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("daniel2005");
                context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEED ERROR] Error al sembrar base de datos: {ex.Message}");
        }
    }
}
