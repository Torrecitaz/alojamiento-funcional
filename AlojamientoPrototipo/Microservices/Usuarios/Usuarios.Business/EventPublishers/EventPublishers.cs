namespace Usuarios.Business.EventPublishers;

/// <summary>
/// Stub: Publicará eventos cuando se cree/actualice un usuario.
/// Se conectará a RabbitMQ en la Fase 4.
/// </summary>
public class UsuarioEventPublisher
{
    public Task PublishUsuarioCreatedAsync(int usuarioId, string email) => Task.CompletedTask;
    public Task PublishUsuarioUpdatedAsync(int usuarioId) => Task.CompletedTask;
    public Task PublishUsuarioDeletedAsync(int usuarioId) => Task.CompletedTask;
}

public class ClienteEventPublisher
{
    public Task PublishClienteRegisteredAsync(int clienteId, string cedula) => Task.CompletedTask;
    public Task PublishClienteUpdatedAsync(int clienteId) => Task.CompletedTask;
}

public class AuthEventPublisher
{
    public Task PublishLoginSuccessAsync(int usuarioId) => Task.CompletedTask;
    public Task PublishLoginFailedAsync(string email) => Task.CompletedTask;
}
