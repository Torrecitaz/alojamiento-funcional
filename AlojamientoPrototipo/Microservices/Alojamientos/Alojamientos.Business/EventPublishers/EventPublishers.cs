namespace Alojamientos.Business.EventPublishers;

/// <summary>
/// Stub para Event Publishers de RabbitMQ (Fase 4)
/// </summary>
public class AlojamientoEventPublisher
{
    public Task PublishAlojamientoCreadoAsync(int alojamientoId) => Task.CompletedTask;
    public Task PublishAlojamientoActualizadoAsync(int alojamientoId) => Task.CompletedTask;
    public Task PublishAlojamientoEliminadoAsync(int alojamientoId) => Task.CompletedTask;
}
