using Microsoft.EntityFrameworkCore;
using Alojamientos.DataAccess.Contexts;
using Alojamientos.API.Extensions;
using Alojamientos.API.Middleware;
using MassTransit;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ── 1. Base de datos ─────────────────────────────────
builder.Services.AddDbContext<AlojamientosDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConexionAlojamientos"))
           .UseLowerCaseNamingConvention());

// ── 2. Dependencias de la Aplicación ─────────────────
builder.Services.AddApplicationServices();

// ── Event Bus (MassTransit + RabbitMQ) ────────────────
builder.Services.AddMassTransit(x =>
{
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

builder.Services.Configure<MassTransitHostOptions>(options =>
{
    options.WaitUntilStarted = false;
    options.StartTimeout = TimeSpan.FromSeconds(30);
});

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
