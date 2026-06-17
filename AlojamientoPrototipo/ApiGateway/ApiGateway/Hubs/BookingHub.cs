using Microsoft.AspNetCore.SignalR;

namespace ApiGateway.Hubs;

public class BookingHub : Hub
{
    private readonly ILogger<BookingHub> _logger;

    public BookingHub(ILogger<BookingHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("🔌 Cliente conectado al Hub de SignalR: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("🔌 Cliente desconectado del Hub de SignalR: {ConnectionId}, Error: {Error}", Context.ConnectionId, exception?.Message);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGroup(string groupName)
    {
        if (!string.IsNullOrEmpty(groupName))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("🔌 Cliente {ConnectionId} se unió al grupo: {GroupName}", Context.ConnectionId, groupName);
        }
    }
}
