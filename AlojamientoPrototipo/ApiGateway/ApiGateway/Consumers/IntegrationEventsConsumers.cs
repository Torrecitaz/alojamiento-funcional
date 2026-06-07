using MassTransit;
using Microsoft.AspNetCore.SignalR;
using ApiGateway.Hubs;
using Shared.Kernel.Events;

namespace ApiGateway.Consumers;

public class ReservaCreatedConsumer : IConsumer<ReservaCreatedEvent>
{
    private readonly IHubContext<BookingHub> _hubContext;
    private readonly ILogger<ReservaCreatedConsumer> _logger;

    public ReservaCreatedConsumer(IHubContext<BookingHub> hubContext, ILogger<ReservaCreatedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReservaCreatedEvent> context)
    {
        _logger.LogInformation("📢 Consumiendo ReservaCreatedEvent: {CodigoReserva}", context.Message.CodigoReserva);
        await _hubContext.Clients.All.SendAsync("OnReservaCreated", context.Message);
    }
}

public class ReservaConfirmedConsumer : IConsumer<ReservaConfirmedEvent>
{
    private readonly IHubContext<BookingHub> _hubContext;
    private readonly ILogger<ReservaConfirmedConsumer> _logger;

    public ReservaConfirmedConsumer(IHubContext<BookingHub> hubContext, ILogger<ReservaConfirmedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReservaConfirmedEvent> context)
    {
        _logger.LogInformation("📢 Consumiendo ReservaConfirmedEvent: {CodigoReserva}", context.Message.CodigoReserva);
        await _hubContext.Clients.All.SendAsync("OnReservaConfirmed", context.Message);
    }
}

public class ReservaCancelledConsumer : IConsumer<ReservaCancelledEvent>
{
    private readonly IHubContext<BookingHub> _hubContext;
    private readonly ILogger<ReservaCancelledConsumer> _logger;

    public ReservaCancelledConsumer(IHubContext<BookingHub> hubContext, ILogger<ReservaCancelledConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReservaCancelledEvent> context)
    {
        _logger.LogInformation("📢 Consumiendo ReservaCancelledEvent: {CodigoReserva}", context.Message.CodigoReserva);
        await _hubContext.Clients.All.SendAsync("OnReservaCancelled", context.Message);
    }
}

public class HabitacionDisponibilidadChangedConsumer : IConsumer<HabitacionDisponibilidadChangedEvent>
{
    private readonly IHubContext<BookingHub> _hubContext;
    private readonly ILogger<HabitacionDisponibilidadChangedConsumer> _logger;

    public HabitacionDisponibilidadChangedConsumer(IHubContext<BookingHub> hubContext, ILogger<HabitacionDisponibilidadChangedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<HabitacionDisponibilidadChangedEvent> context)
    {
        _logger.LogInformation("📢 Consumiendo HabitacionDisponibilidadChangedEvent: Habitacion={HabitacionId}, Fecha={Fecha}, Estado={Estado}", 
            context.Message.HabitacionId, context.Message.Fecha, context.Message.Estado);
        await _hubContext.Clients.All.SendAsync("OnAvailabilityChanged", context.Message);
    }
}

public class AlojamientoEstadoChangedConsumer : IConsumer<AlojamientoEstadoChangedEvent>
{
    private readonly IHubContext<BookingHub> _hubContext;
    private readonly ILogger<AlojamientoEstadoChangedConsumer> _logger;

    public AlojamientoEstadoChangedConsumer(IHubContext<BookingHub> hubContext, ILogger<AlojamientoEstadoChangedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AlojamientoEstadoChangedEvent> context)
    {
        _logger.LogInformation("📢 Consumiendo AlojamientoEstadoChangedEvent: Alojamiento={AlojamientoId}, Estado={Estado}", 
            context.Message.AlojamientoId, context.Message.Estado);
        await _hubContext.Clients.All.SendAsync("OnAlojamientoEstadoChanged", context.Message);
    }
}
