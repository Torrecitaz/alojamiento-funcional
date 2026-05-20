using Microsoft.EntityFrameworkCore;
using Usuarios.DataAccess.Contexts;
using Usuarios.API.Extensions;
using Usuarios.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Base de datos ─────────────────────────────────
builder.Services.AddDbContext<UsuariosDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConexionUsuarios"), 
            npgsqlOptions => npgsqlOptions.UseNetTopologySuite())
           .UseLowerCaseNamingConvention());

// ── 2. Dependencias de la Aplicación ─────────────────
builder.Services.AddApplicationServices();

// ── 3. Presentación (Controllers) ────────────────────
builder.Services.AddControllers();

// ── 4. Infraestructura Web (Swagger & CORS) ──────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwagger();
builder.Services.AddCustomCors();

var app = builder.Build();

// ── Pipeline ─────────────────────────────────────────

// Manejo Global de Excepciones
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger (siempre activo para el prototipo)
app.UseSwagger();
app.UseSwaggerUI();

// CORS
app.UseCors();

// Mapeo de Controladores
app.MapControllers();

app.Run();
