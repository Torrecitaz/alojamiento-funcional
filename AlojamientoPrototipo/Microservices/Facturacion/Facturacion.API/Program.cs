using Microsoft.EntityFrameworkCore;
using Facturacion.DataAccess.Contexts;
using Facturacion.API.Extensions;
using Facturacion.API.Middleware;
using MassTransit;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ── 1. Base de datos ─────────────────────────────────
var conexionFacturacion = builder.Configuration.GetConnectionString("ConexionFacturacion");
if (!string.IsNullOrEmpty(conexionFacturacion) && !conexionFacturacion.Contains("Maximum Pool Size", StringComparison.OrdinalIgnoreCase))
{
    conexionFacturacion = conexionFacturacion.TrimEnd(';') + ";Maximum Pool Size=3;";
}
builder.Services.AddDbContext<FacturacionDbContext>(options =>
    options.UseNpgsql(conexionFacturacion)
           .UseLowerCaseNamingConvention());

// ── 2. Dependencias de la Aplicación ─────────────────
builder.Services.AddApplicationServices();

// ── Event Bus (MassTransit + RabbitMQ) ───────────────
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        // Se espera "amqps://user:pass@host/vhost" desde appsettings.json o variables de entorno
        var rmqUrl = builder.Configuration.GetConnectionString("RabbitMQ");
        if (!string.IsNullOrEmpty(rmqUrl))
        {
            cfg.Host(new Uri(rmqUrl));
        }
        else
        {
            // Fallback para desarrollo local si no hay nube configurada
            cfg.Host("localhost", "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });
        }
        
        // Configurar política de reintentos exponencial
        cfg.UseMessageRetry(r => r.Exponential(
            5,
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(5)
        ));
        
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.Configure<MassTransitHostOptions>(options =>
{
    options.WaitUntilStarted = false;
    options.StartTimeout = TimeSpan.FromSeconds(30);
});

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

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// CORS
app.UseCors();

// Mapeo de Controladores
app.MapControllers();

app.Run();
