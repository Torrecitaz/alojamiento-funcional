using Microsoft.AspNetCore.Mvc;
using ApiGateway.Models;
using ApiGateway.Models.Internal;
using System.Text.Json;
using MassTransit;
using ApiGateway.Hubs;
using ApiGateway.Consumers;
using ApiGateway.Endpoints;
using ApiGateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── Sanitización de URLs de Microservicios ──
void SanitizeConfigurationUrl(string key)
{
    var value = builder.Configuration[key];
    if (!string.IsNullOrEmpty(value))
    {
        if (!value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
            !value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            builder.Configuration[key] = "http://" + value;
        }
    }
}

SanitizeConfigurationUrl("Microservices:UsuariosUrl");
SanitizeConfigurationUrl("Microservices:AlojamientosUrl");
SanitizeConfigurationUrl("Microservices:ReservasUrl");
SanitizeConfigurationUrl("Microservices:FacturacionUrl");

SanitizeConfigurationUrl("ReverseProxy:Clusters:usuarios-cluster:Destinations:destination1:Address");
SanitizeConfigurationUrl("ReverseProxy:Clusters:alojamientos-cluster:Destinations:destination1:Address");
SanitizeConfigurationUrl("ReverseProxy:Clusters:reservas-cluster:Destinations:destination1:Address");
SanitizeConfigurationUrl("ReverseProxy:Clusters:facturacion-cluster:Destinations:destination1:Address");


// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AlojamientoMR - Contrato de Integración para Booking",
        Version = "1.0.0",
        Description = "API pública orientada al flujo del usuario final dentro de la plataforma Booking."
    });
});

// Configure Named HttpClients for microservices
builder.Services.AddHttpClient("Usuarios", client =>
{
    var url = builder.Configuration["Microservices:UsuariosUrl"] ?? "http://localhost:5001";
    client.BaseAddress = new Uri(url);
});

builder.Services.AddHttpClient("Alojamientos", client =>
{
    var url = builder.Configuration["Microservices:AlojamientosUrl"] ?? "http://localhost:5002";
    client.BaseAddress = new Uri(url);
});

builder.Services.AddHttpClient("Reservas", client =>
{
    var url = builder.Configuration["Microservices:ReservasUrl"] ?? "http://localhost:5003";
    client.BaseAddress = new Uri(url);
});

builder.Services.AddHttpClient("Facturacion", client =>
{
    var url = builder.Configuration["Microservices:FacturacionUrl"] ?? "http://localhost:5004";
    client.BaseAddress = new Uri(url);
});

// Habilitar SignalR y Bus de Eventos con MassTransit
builder.Services.AddSignalR();
builder.Services.AddMassTransit(x =>
{
    // Local signalr/event consumers
    x.AddConsumer<ReservaCreatedConsumer>();
    x.AddConsumer<ReservaConfirmedConsumer>();
    x.AddConsumer<ReservaCancelledConsumer>();
    x.AddConsumer<HabitacionDisponibilidadChangedConsumer>();
    x.AddConsumer<AlojamientoEstadoChangedConsumer>();

    // Booking sync consumers
    x.AddConsumer<BookingSyncReservaCreatedConsumer>();
    x.AddConsumer<BookingSyncReservaCancelledConsumer>();
    x.AddConsumer<BookingSyncAvailabilityConsumer>();

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

// Agregar YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

app.MapHub<BookingHub>("/bookingHub");

// ── Interceptor de checkout ANTES de YARP ──
app.UseMiddleware<CheckoutMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Public API Gateway v1");
    });
}

// ── Mapear Endpoints Modularizados ──
app.MapUsuarioEndpoints();
app.MapPropiedadesEndpoints();
app.MapHabitacionEndpoints();
app.MapReservaEndpoints();
app.MapFacturacionEndpoints();
app.MapBookingIntegrationEndpoints();

app.MapReverseProxy();

app.Run();
