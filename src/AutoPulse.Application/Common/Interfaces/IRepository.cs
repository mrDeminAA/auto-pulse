using System.Linq.Expressions;

namespace AutoPulse.Application.Common.Interfaces;

/// <summary>
/// Базовый репозиторий с основными CRUD операциями
/// </summary>
/// <typeparam name="T">Тип сущности</typeparam>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    
    // Specification Pattern
    Task<IReadOnlyList<T>> FindBySpecificationAsync(ISpecification<T> specification, CancellationToken ct = default);
    Task<int> CountBySpecificationAsync(ISpecification<T> specification, CancellationToken ct = default);
}
