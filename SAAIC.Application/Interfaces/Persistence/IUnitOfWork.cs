using System.Data;

namespace SAAIC.Application.Interfaces.Persistence;

/// <summary>
/// Unidad de trabajo para confirmar cambios y coordinar transacciones atómicas en SQL Server
/// (indispensable para prevenir Race Conditions al reservar espacios).
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inicia una transacción explícita con el nivel de aislamiento indicado.
    /// </summary>
    Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
