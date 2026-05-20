using Microsoft.EntityFrameworkCore;
using Reservas.DataAccess.Contexts;
using Reservas.API.Extensions;
using Reservas.API.Middleware;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Base de datos ─────────────────────────────────
builder.Services.AddDbContext<ReservasDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConexionReservas"))
           .UseLowerCaseNamingConvention());

// ── 2. Dependencias de la Aplicación ─────────────────
builder.Services.AddApplicationServices(builder.Configuration);

// ── Event Bus (MassTransit + RabbitMQ / CloudAMQP) ───
builder.Services.AddMassTransit(x =>
{
    // Registrar el consumidor de FacturaPagadaEvent
    x.AddConsumer<Reservas.API.Consumers.FacturaPagadaConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rmqUrl = builder.Configuration.GetConnectionString("RabbitMQ");
        if (!string.IsNullOrEmpty(rmqUrl))
        {
            cfg.Host(new Uri(rmqUrl));
        }
        else
        {
            cfg.Host("localhost", "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });
        }

        cfg.ConfigureEndpoints(context);
    });
});

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

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// CORS
app.UseCors();

// Mapeo de Controladores
app.MapControllers();

app.Run();
