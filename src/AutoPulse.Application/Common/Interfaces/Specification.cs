using System.Linq.Expressions;
using AutoPulse.Application.Common.Interfaces;

namespace AutoPulse.Application.Common.Interfaces;

/// <summary>
/// Базовый класс для спецификаций
/// </summary>
/// <typeparam name="T">Тип сущности</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    protected virtual void AddCriteria(Expression<Func<T, bool>> criteria)
    {
        if (Criteria is null)
        {
            Criteria = criteria;
        }
        else
        {
            Criteria = Criteria.AndAlso(criteria);
        }
    }

    protected virtual void AddOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected virtual void AddOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }

    protected virtual void AddPaging(int take, int skip)
    {
        Take = take;
        Skip = skip;
        IsPagingEnabled = true;
    }
}

/// <summary>
/// Extension methods для комбинирования выражений
/// </summary>
public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> AndAlso<T>(
        this Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        var parameter = first.Parameters[0];
        var visitor = new ParameterReplacer(parameter);
        var body = Expression.AndAlso(
            first.Body,
            visitor.Visit(second.Body));
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;

        public ParameterReplacer(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(_parameter);
        }
    }
}
