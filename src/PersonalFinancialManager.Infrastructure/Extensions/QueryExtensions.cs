namespace PersonalFinancialManager.Infrastructure.Extensions;

using System.Linq.Expressions;
using System.Reflection;

public static class QueryExtensions
{
    public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, string propertyName, string orderType)
    {
        PropertyInfo? propertyInfo = typeof(TSource).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        ArgumentNullException.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        ParameterExpression parameter = Expression.Parameter(typeof(TSource), "x");
        MemberExpression propertyAccess = Expression.Property(parameter, propertyInfo);
        LambdaExpression orderByExpression = Expression.Lambda(propertyAccess, parameter);

        string methodName = orderType.ToUpper() switch
        {
            "ASC" => "OrderBy",
            "DESC" => "OrderByDescending",
            _ => "OrderBy"
        };

        MethodInfo method = typeof(Queryable).GetMethods()
            .Where(m => m.Name == methodName && m.GetParameters().Length == 2)
            .Single()
            .MakeGenericMethod(typeof(TSource), propertyInfo.PropertyType);

        MethodCallExpression resultExpression = Expression.Call(method, source.Expression, Expression.Quote(orderByExpression));

        return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>(resultExpression);
    }

    public static IOrderedQueryable<TSource> ThanBy<TSource>(this IQueryable<TSource> source, string propertyName, string orderType)
    {
        PropertyInfo? propertyInfo = typeof(TSource).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        ArgumentNullException.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        ParameterExpression parameter = Expression.Parameter(typeof(TSource), "x");
        MemberExpression propertyAccess = Expression.Property(parameter, propertyInfo);
        LambdaExpression orderByExpression = Expression.Lambda(propertyAccess, parameter);

        string methodName = orderType.ToUpper() switch
        {
            "ASC" => "ThanBy",
            "DESC" => "ThanByDescending",
            _ => "OrderBy"
        };

        MethodInfo method = typeof(Queryable).GetMethods()
            .Where(m => m.Name == methodName && m.GetParameters().Length == 2)
            .Single()
            .MakeGenericMethod(typeof(TSource), propertyInfo.PropertyType);

        MethodCallExpression resultExpression = Expression.Call(method, source.Expression, Expression.Quote(orderByExpression));

        return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>(resultExpression);
    }
}
