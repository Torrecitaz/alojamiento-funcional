using System;
using BookingIntegration.API.Consumers;
using BookingIntegration.API.Data;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Register Data & DB Helper ──────────────────────
builder.Services.AddSingleton<BookingDbHelper>();

// ── 2. Configure HTTP Clients for Microservices ────────
builder.Services.AddHttpClient("Usuarios", client =>
{
    var url = builder.Configuration["Microservices:UsuariosUrl"] ?? "http://localhost:5001";
    client.BaseAddress = new Uri(url.TrimEnd('/') + "/");
});

builder.Services.AddHttpClient("Alojamientos", client =>
{
    var url = builder.Configuration["Microservices:AlojamientosUrl"] ?? "http://localhost:5002";
    client.BaseAddress = new Uri(url.TrimEnd('/') + "/");
});

builder.Services.AddHttpClient("Reservas", client =>
{
    var url = builder.Configuration["Microservices:ReservasUrl"] ?? "http://localhost:5003";
    client.BaseAddress = new Uri(url.TrimEnd('/') + "/");
});

// ── 3. Configure MassTransit (RabbitMQ / CloudAMQP) ──
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BookingSyncReservaConfirmedConsumer>();
    x.AddConsumer<BookingSyncReservaCancelledConsumer>();
    x.AddConsumer<BookingSyncHabitacionDisponibilidadChangedConsumer>();
    x.AddConsumer<BookingSyncAlojamientoEstadoChangedConsumer>();

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

        // Auto-configure endpoints for consumers
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.Configure<MassTransitHostOptions>(options =>
{
    options.WaitUntilStarted = false;
    options.StartTimeout = TimeSpan.FromSeconds(30);
});

// ── 4. Web API setup ──────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(p =>
    {
        p.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod();
    });
});

var app = builder.Build();

// ── 5. Initialize Simulated Booking DB Metadata ───────
using (var scope = app.Services.CreateScope())
{
    var dbHelper = scope.ServiceProvider.GetRequiredService<BookingDbHelper>();
    await dbHelper.InitializeAsync();
}

// ── 6. Middleware Pipeline ───────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
