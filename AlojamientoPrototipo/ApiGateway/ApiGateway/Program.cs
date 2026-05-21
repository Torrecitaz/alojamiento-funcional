var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Public API Gateway v1");
    });
}

app.MapPost("/api/v1/alojaexpress/booking", (ApiGateway.Models.CrearReservaRequest request) =>
    Results.Ok(new ApiGateway.Models.ReservaResponse()))
    .WithName("CreateBooking")
    .WithTags("Booking")
    .WithSummary("Crear una nueva reserva")
    .WithOpenApi();

app.MapPost("/api/v1/mateo-torres/booking", (ApiGateway.Models.CrearReservaRequest request) =>
    Results.Ok(new ApiGateway.Models.ReservaResponse()))
    .WithName("CreateBookingMateoTorres")
    .WithTags("Booking")
    .WithSummary("Crear una nueva reserva (Mateo Torres)")
    .WithOpenApi();

app.MapPost("/api/v1/Mateo Torres/booking", (ApiGateway.Models.CrearReservaRequest request) =>
    Results.Ok(new ApiGateway.Models.ReservaResponse()))
    .WithName("CreateBookingMateoTorresSpace")
    .WithTags("Booking")
    .WithSummary("Crear una nueva reserva (Mateo Torres con espacio)")
    .WithOpenApi();

app.MapReverseProxy();

app.Run();
