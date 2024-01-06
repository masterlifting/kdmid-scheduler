using System;
using System.Linq.Expressions;

namespace Net.Shared.Extensions;

public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> Combine<T>(this Expression<Func<T, bool>> firstExpression, Expression<Func<T, bool>> secondExpression)
    {
        var invokedExpression = Expression.Invoke(secondExpression, firstExpression.Parameters);
        var combinedExpression = Expression.AndAlso(firstExpression.Body, invokedExpression);
        return Expression.Lambda<Func<T, bool>>(combinedExpression, firstExpression.Parameters);
    }
}
