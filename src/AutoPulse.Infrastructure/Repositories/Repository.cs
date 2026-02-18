using System.Linq.Expressions;
using AutoPulse.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoPulse.Infrastructure.Repositories;

/// <summary>
/// Базовая реализация репозитория
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(DbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await DbSet.FindAsync(new object[] { id }, ct);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await DbSet.ToListAsync(ct);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(predicate, ct);
    }

    public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await DbSet.Where(predicate).ToListAsync(ct);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await DbSet.AddAsync(entity, ct);
        return entity;
    }

    public virtual void Update(T entity)
    {
        Context.Entry(entity).State = EntityState.Modified;
    }

    public virtual void Remove(T entity)
    {
        DbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        return predicate == null
            ? await DbSet.CountAsync(ct)
            : await DbSet.CountAsync(predicate, ct);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await DbSet.AnyAsync(predicate, ct);
    }

    public virtual async Task<IReadOnlyList<T>> FindBySpecificationAsync(ISpecification<T> specification, CancellationToken ct = default)
    {
        var query = ApplySpecification(specification);
        return await query.ToListAsync(ct);
    }

    public virtual async Task<int> CountBySpecificationAsync(ISpecification<T> specification, CancellationToken ct = default)
    {
        var query = ApplySpecification(specification);
        return await query.CountAsync(ct);
    }

    protected IQueryable<T> ApplySpecification(ISpecification<T> specification)
    {
        var query = DbSet.AsQueryable();

        // Применяем критерий фильтрации
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        // Применяем Include
        query = specification.Includes.Aggregate(query,
            (current, include) => current.Include(include));

        // Применяем строковые Include (для then include)
        query = specification.IncludeStrings.Aggregate(query,
            (current, include) => current.Include(include));

        // Применяем сортировку
        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Применяем пагинацию
        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip).Take(specification.Take);
        }

        return query;
    }
}
