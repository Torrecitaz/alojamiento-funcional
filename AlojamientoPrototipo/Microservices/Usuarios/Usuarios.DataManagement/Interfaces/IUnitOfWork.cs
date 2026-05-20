namespace Usuarios.DataManagement.Interfaces;

/// <summary>
/// Patrón Unit of Work: agrupa múltiples operaciones de repositorio
/// en una sola transacción. Garantiza atomicidad.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
