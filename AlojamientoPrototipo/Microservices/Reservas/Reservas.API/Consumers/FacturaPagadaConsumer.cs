using MassTransit;
using Shared.Kernel.Events;
using Reservas.Business.Interfaces;
using Reservas.Business.DTOs;
using Reservas.Business.Exceptions;

namespace Reservas.API.Consumers;

/// <summary>
/// Consumidor que escucha el evento FacturaPagadaEvent emitido por el microservicio de Facturación.
/// Al recibir este evento, actualiza el estado de la reserva de "Pendiente" a "Confirmada".
/// </summary>
public class FacturaPagadaConsumer : IConsumer<FacturaPagadaEvent>
{
    private readonly IReservasService _reservasService;
    private readonly ILogger<FacturaPagadaConsumer> _logger;

    public FacturaPagadaConsumer(
        IReservasService reservasService,
        ILogger<FacturaPagadaConsumer> logger)
    {
        _reservasService = reservasService;
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
            try
            {
                var reserva = await _reservasService.GetByIdAsync(evento.ReservaId);
                if (reserva != null && reserva.Estado.Equals("Confirmada", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation(
                        "ℹ️ Reserva {ReservaId} ya se encuentra 'Confirmada'. Se ignora el evento de forma idempotente.",
                        evento.ReservaId);
                    return;
                }
            }
            catch (ReservaNotFoundException)
            {
                _logger.LogWarning(
                    "⚠️ ReservaId={ReservaId} no encontrada en el sistema. Se descarta el evento.",
                    evento.ReservaId);
                return;
            }

            // Actualizar estado de la reserva a "Confirmada" llamando al servicio para publicar los eventos de integración
            await _reservasService.ActualizarEstadoAsync(evento.ReservaId, new ActualizarEstadoReservaRequest("Confirmada"));

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
