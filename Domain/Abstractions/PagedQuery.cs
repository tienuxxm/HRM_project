using System.Linq.Expressions;
using System.Reflection;
using Domain.Helpers;

namespace Domain.Abstractions;

public abstract record PagedQuery<TEntity, TEntityId>
{
    private const int MaxPageSize = 500;
    private int _page = 1;
    private int _pageSize = 10;

    public required int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public required int Page
    {
        get { return _page; }
        set { _page = value > 0 ? value : 1; }
    }

    public string? SearchTerm { get; set; }
    public string? SortOrder { get; set; }
    public string? SortColumn { get; set; }

    public OrderByType OrderByType => string.IsNullOrEmpty(SortOrder)
        ? OrderByType.DESC
        : SortOrder.ToUpper() switch
        {
            "ASC" => OrderByType.ASC,
            "DESC" => OrderByType.DESC,
            _ => OrderByType.DESC
        };

    public Expression<Func<TEntity, object>>? SortExpression
    {
        get
        {
            if (string.IsNullOrEmpty(SortColumn))
            {
                var idPropertyName = typeof(TEntity).GetProperties().First(x => x.PropertyType == typeof(TEntityId))
                    .Name;
                return GetLambdaExpression(idPropertyName);
            }

            var propertyNames = typeof(TEntity).GetProperties().Select(x => x.Name).ToList();
            return !propertyNames.Contains(SortColumn) ? null : GetLambdaExpression(SortColumn);
        }
    }

    private static Expression<Func<TEntity, object>>? GetLambdaExpression(string propertyName)
    {
        var entityType = typeof(TEntity);
        var parameter = Expression.Parameter(entityType, "x");
        var propertyInfo = typeof(TEntity).GetProperty(propertyName);
        if (propertyInfo == null) return null;
        var propertyAccess = Expression.Property(parameter, propertyInfo);
        // Replace the parameter in the originalExpression with the new parameter
        var expressionBody = new ParameterReplacer(parameter).Visit(propertyAccess);
        var lambda = Expression.Lambda<Func<TEntity, object>>(
            expressionBody.Type.IsValueType ? Expression.Convert(expressionBody, typeof(object)) : expressionBody,
            parameter);

        return lambda;
    }
}