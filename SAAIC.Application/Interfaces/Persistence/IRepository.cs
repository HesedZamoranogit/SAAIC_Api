using System.Linq.Expressions;
using SAAIC.Domain.Common;

namespace SAAIC.Application.Interfaces.Persistence;

/// <summary>
/// Contrato base de repositorio generico para operaciones comunes.
/// </summary>
/// <typeparam name="T">Tipo de entidad que hereda de BaseEntity.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}
