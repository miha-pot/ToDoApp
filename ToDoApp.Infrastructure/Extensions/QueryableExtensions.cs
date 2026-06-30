using System.Linq.Expressions;
using System.Reflection;
using ToDoApp.Shared.Common.Filtering;

namespace ToDoApp.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query,
                                                List<QueryFilter> filters)
    {
        foreach (var filter in filters)
        {
            var predicate = BuildPredicate<T>(filter);
            if (predicate is not null)
                query = query.Where(predicate);
        }

        return query;
    }

    public static IQueryable<T> ApplySort<T>(this IQueryable<T> query,
                                             string? sortBy,
                                             bool descending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query;

        var bindingFlags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;
        var prop = typeof(T).GetProperty(sortBy, bindingFlags);

        if (prop is null)
            return query;

        var param = Expression.Parameter(typeof(T), "x");
        var body = Expression.PropertyOrField(param, prop.Name);
        var keySelector = Expression.Lambda(body, param);

        var method = descending ? "OrderByDescending" : "OrderBy";

        var result = typeof(Queryable).GetMethods()
                                      .First(m => m.Name == method && m.GetParameters().Length == 2)
                                      .MakeGenericMethod(typeof(T), prop.PropertyType)
                                      .Invoke(null, [query, keySelector]);

        return (IQueryable<T>)result!;
    }

    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query,
                                               int page,
                                               int pageSize)
    {
        return query.Skip((page - 1) * pageSize)
                    .Take(pageSize);
    }

    private static Expression<Func<T, bool>>? BuildPredicate<T>(QueryFilter filter)
    {
        var bindingFlags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;
        var prop = typeof(T).GetProperty(filter.Field, bindingFlags);

        if (prop is null)
            return null;

        var param = Expression.Parameter(typeof(T), "x");
        var member = Expression.Property(param, prop);

        object? converted;
        try
        {
            converted = Convert.ChangeType(filter.Value,
                                           Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        }
        catch
        {
            return null;
        }

        var constant = Expression.Constant(converted, prop.PropertyType);

        Expression body = filter.Operator switch
        {
            QueryFilterOperator.Equals => Expression.Equal(member, constant),
            QueryFilterOperator.NotEquals => Expression.NotEqual(member, constant),
            QueryFilterOperator.GreaterThan => Expression.GreaterThan(member, constant),
            QueryFilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(member, constant),
            QueryFilterOperator.LessThan => Expression.LessThan(member, constant),
            QueryFilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(member, constant),

            QueryFilterOperator.Contains => StringMethod(member, constant, "Contains"),
            QueryFilterOperator.StartsWith => StringMethod(member, constant, "StartsWith"),
            QueryFilterOperator.EndsWith => StringMethod(member, constant, "EndsWith"),

            _ => throw new NotSupportedException($"Operator {filter.Operator} is not supported.")
        };

        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    private static MethodCallExpression StringMethod(MemberExpression member,
                                                     ConstantExpression constant,
                                                     string methodName)
    {
        var method = typeof(string).GetMethod(methodName, [typeof(string)])
            ?? throw new InvalidOperationException($"String method '{methodName}' not found.");

        return Expression.Call(member, method, constant);
    }
}
