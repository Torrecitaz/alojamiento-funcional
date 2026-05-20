using MassTransit;
using Shared.Kernel.Events;
using Reservas.DataManagement.Interfaces;

namespace Reservas.API.Consumers;

/// <summary>
/// Consumidor que escucha el evento FacturaPagadaEvent emitido por el microservicio de Facturación.
/// Al recibir este evento, actualiza el estado de la reserva de "Pendiente" a "Confirmada".
/// </summary>
public class FacturaPagadaConsumer : IConsumer<FacturaPagadaEvent>
{
    private readonly IReservasDataService _reservasDataService;
    private readonly ILogger<FacturaPagadaConsumer> _logger;

    public FacturaPagadaConsumer(
        IReservasDataService reservasDataService,
        ILogger<FacturaPagadaConsumer> logger)
    {
        _reservasDataService = reservasDataService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FacturaPagadaEvent> context)
    {
        var evento = context.Message;

        _logger.LogInformation(
            "📩 Evento FacturaPagadaEvent recibido: ReservaId={ReservaId}, FacturaId={FacturaId}, Monto={Monto}",
            evento.ReservaId, evento.FacturaId, evento.MontoPagado);

        try
        {
            // Actualizar estado de la reserva a "Confirmada"
            await _reservasDataService.UpdateStatusAsync(evento.ReservaId, "Confirmada");

            _logger.LogInformation(
                "✅ Reserva {ReservaId} actualizada a 'Confirmada' tras pago de factura {FacturaId}",
                evento.ReservaId, evento.FacturaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Error al procesar FacturaPagadaEvent para ReservaId={ReservaId}",
                evento.ReservaId);
            throw; // MassTransit reintentará según su política de retry
        }
    }
}
