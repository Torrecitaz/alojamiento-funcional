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

// Estos endpoints son puramente para generar el Swagger con el contrato público requerido.
// YARP interceptará la llamada real y la enviará al microservicio correspondiente.
app.MapPost("/api/v1/mathias-rivera/booking", (ApiGateway.Models.CrearReservaRequest request) => 
    Results.Ok(new ApiGateway.Models.ReservaResponse()))
    .WithName("CreateBooking")
    .WithTags("Booking")
    .WithSummary("Crear una nueva reserva")
    .WithOpenApi();

app.MapReverseProxy();

app.Run();
