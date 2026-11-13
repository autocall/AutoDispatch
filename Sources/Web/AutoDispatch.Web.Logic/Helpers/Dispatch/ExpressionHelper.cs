using System.Linq.Expressions;

namespace AutoDispatch.Web.Logic.Helpers.Dispatch;

/// <summary>
///     Склейка предикатов (V1 DispatchService.CombineLambdas): фильтры грузов по доскам и истории </summary>
public static class ExpressionHelper {

    public static Expression<Func<T, bool>> CombineLambdas<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right, ExpressionType expressionType) {
        // Remove expressions created with Begin<T>()
        if (left.Body.NodeType == ExpressionType.Constant) {
            return right;
        }
        ParameterExpression p = left.Parameters[0];
        var visitor = new SubstituteParameterVisitor();
        visitor.Sub[right.Parameters[0]] = p;
        Expression body = Expression.MakeBinary(expressionType, left.Body, visitor.Visit(right.Body));
        return Expression.Lambda<Func<T, bool>>(body, p);
    }

    private class SubstituteParameterVisitor : ExpressionVisitor {
        public Dictionary<Expression, Expression> Sub = new Dictionary<Expression, Expression>();

        protected override Expression VisitParameter(ParameterExpression node) {
            return Sub.TryGetValue(node, out var newValue) ? newValue : node;
        }
    }
}
