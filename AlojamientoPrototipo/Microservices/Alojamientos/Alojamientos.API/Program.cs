using Microsoft.EntityFrameworkCore;
using Alojamientos.DataAccess.Contexts;
using Alojamientos.API.Extensions;
using Alojamientos.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Base de datos ─────────────────────────────────
builder.Services.AddDbContext<AlojamientosDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConexionAlojamientos"),
            npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null))
           .UseLowerCaseNamingConvention());

// ── 2. Dependencias de la Aplicación ─────────────────
builder.Services.AddApplicationServices();

// ── 3. Presentación (Controllers & gRPC) ───────────────
builder.Services.AddControllers();
builder.Services.AddGrpc();

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
app.UseRouting();

// Mapeo de Controladores
app.MapControllers();

// gRPC Service
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.MapGrpcService<Alojamientos.API.GrpcServices.CalendarioGrpcService>()
   .EnableGrpcWeb();

app.Run();
