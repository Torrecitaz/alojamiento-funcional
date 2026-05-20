using Microsoft.EntityFrameworkCore.Storage;
using Alojamientos.DataAccess.Contexts;
using Alojamientos.DataManagement.Interfaces;

namespace Alojamientos.DataManagement.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly AlojamientosDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AlojamientosDbContext context) => _context = context;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync()
        => _transaction = await _context.Database.BeginTransactionAsync();

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
