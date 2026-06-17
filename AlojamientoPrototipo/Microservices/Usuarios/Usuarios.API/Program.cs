using Microsoft.EntityFrameworkCore;
using Usuarios.DataAccess.Contexts;
using Usuarios.API.Extensions;
using Usuarios.API.Middleware;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ── 1. Base de datos ─────────────────────────────────
var conexionUsuarios = builder.Configuration.GetConnectionString("ConexionUsuarios");
if (!string.IsNullOrEmpty(conexionUsuarios) && !conexionUsuarios.Contains("Maximum Pool Size", StringComparison.OrdinalIgnoreCase))
{
    conexionUsuarios = conexionUsuarios.TrimEnd(';') + ";Maximum Pool Size=3;";
}
builder.Services.AddDbContext<UsuariosDbContext>(options =>
    options.UseNpgsql(conexionUsuarios, 
            npgsqlOptions => {
                npgsqlOptions.UseNetTopologySuite();
            })
           .UseLowerCaseNamingConvention());

// ── 2. Dependencias de la Aplicación ─────────────────
builder.Services.AddApplicationServices();

// ── 3. Presentación (Controllers) ────────────────────
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

// ── 4. Infraestructura Web (Swagger & CORS) ──────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwagger();
builder.Services.AddCustomCors();

var app = builder.Build();

// ── Pipeline ─────────────────────────────────────────

// Manejo Global de Excepciones
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Health Checks
app.MapHealthChecks("/health");

// Swagger (siempre activo para el prototipo)
app.UseSwagger();
app.UseSwaggerUI();

// CORS
app.UseCors();

// Seed de Base de Datos
app.SeedDatabase();

// Mapeo de Controladores
app.MapControllers();

app.Run();
