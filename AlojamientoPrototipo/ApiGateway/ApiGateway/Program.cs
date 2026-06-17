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
SanitizeConfigurationUrl("Microservices:BookingIntegrationUrl");

SanitizeConfigurationUrl("ReverseProxy:Clusters:usuarios-cluster:Destinations:destination1:Address");
SanitizeConfigurationUrl("ReverseProxy:Clusters:alojamientos-cluster:Destinations:destination1:Address");
SanitizeConfigurationUrl("ReverseProxy:Clusters:reservas-cluster:Destinations:destination1:Address");
SanitizeConfigurationUrl("ReverseProxy:Clusters:facturacion-cluster:Destinations:destination1:Address");


// Add services to the container.
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();

// Register GraphQL Server
builder.Services.AddGraphQLServer()
    .AddQueryType<ApiGateway.GraphQL.Query>()
    .AddType<ApiGateway.GraphQL.AlojamientoType>()
    .AddDataLoader<ApiGateway.GraphQL.HabitacionesDataLoader>()
    .AddDataLoader<ApiGateway.GraphQL.FotosDataLoader>();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AlojamientoMR - Contrato de Integración para Booking",
        Version = "1.0.0",
        Description = "API pública orientada al flujo del usuario final dentro de la plataforma Booking."
    });

    options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AlojamientoMR - Contrato de Integración para Booking - V2",
        Version = "2.0.0",
        Description = "API pública orientada al flujo del usuario final (V2) con soporte para Idempotencia obligatoria en reservas."
    });

    // Add support for X-Idempotency-Key header in Swagger
    options.AddSecurityDefinition("IdempotencyKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "X-Idempotency-Key",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Clave única UUID de idempotencia para la transacción."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "IdempotencyKey"
                }
            },
            Array.Empty<string>()
        }
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

builder.Services.AddHttpClient("BookingIntegration", client =>
{
    var url = builder.Configuration["Microservices:BookingIntegrationUrl"] ?? "http://localhost:5005";
    client.BaseAddress = new Uri(url);
});

// Habilitar SignalR y Bus de Eventos con MassTransit
builder.Services.AddSingleton<Shared.Kernel.Services.ICloudinaryService, Shared.Kernel.Services.CloudinaryService>();
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
        
        // Configurar política de reintentos exponencial (5 reintentos, mín 2s, máx 30s)
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

// Agregar YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:5173", "http://localhost:3000" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

app.MapHealthChecks("/health");

app.MapHub<BookingHub>("/bookingHub");

// ── Middleware de idempotencia y Checkout ANTES de YARP ──
app.UseMiddleware<IdempotencyMiddleware>();
app.UseMiddleware<CheckoutMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Public API Gateway v1");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "Public API Gateway v2");
    });
}

// ── Mapear GraphQL BFF ──
app.MapGraphQL("/graphql");

// ── Mapear Endpoints Modularizados ──
app.MapUsuarioEndpoints();
app.MapPropiedadesEndpoints();
app.MapHabitacionEndpoints();
app.MapReservaEndpoints();
app.MapFacturacionEndpoints();
app.MapBookingIntegrationEndpoints();

app.MapReverseProxy();

app.Run();
