using Microservicio.Clientes.Api.Extensions;
using Microservicio.Clientes.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Render asigna dinámicamente el puerto a través de la variable de entorno PORT. Si no existe (local), usa 5028.
var port = Environment.GetEnvironmentVariable("PORT") ?? "5028";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ── Servicios (Capas DatAccess, DataManagement, Business) ──────────────────
builder.Services.AddBookingServices(builder.Configuration);
builder.Services.AddBookingSwagger();

// Controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Build the App
var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AllowAll");

app.UseBookingSwagger();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
