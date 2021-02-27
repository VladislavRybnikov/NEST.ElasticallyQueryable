using System.Linq.Expressions;
using Nest;
using NEST.ElasticallyQueryable.Internal;
using NEST.ElasticallyQueryable.Internals.Visitors.Base;

namespace NEST.ElasticallyQueryable.Internals.Visitors
{
    public class WhereVisitor<T> : ElasticVisitor
    {
        public WhereVisitor(ISearchDescriptorAccessor accessor) : base(accessor)
        {
        }
        
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.Type == typeof(bool))
            {
                var matchQueryDescriptorType = (typeof(MatchQueryDescriptor<>)).MakeGenericType(typeof(T));

                var queryMethod = SearchDescriptorType.GetMethod(nameof(SearchDescriptor<object>.Query));
                var matchQueryMethod =
                    matchQueryDescriptorType.GetMethod(nameof(MatchQueryDescriptor<object>.Query));
                
                if (matchQueryMethod is null) return node;
                
                MethodCallExpression methodCall = null;

                var queryContainerType = typeof(QueryContainerDescriptor<>).MakeGenericType(typeof(T));

                var queryParam = Expression.Parameter(queryContainerType, "q");

                var matchMethod = typeof(QueryContainerDescriptor<>).MakeGenericType(typeof(T))
                    .GetMethod(nameof(QueryContainerDescriptor<object>.Match));

                if (matchMethod is null) return node;
                
                if (node.NodeType == ExpressionType.Equal)
                {
                    (ConstantExpression constantExpression, MemberExpression propExpression) = (null, null);

                    if (node.Left is ConstantExpression leftConstant
                        && node.Right is MemberExpression rightProperty)
                    {
                        (constantExpression, propExpression) = (leftConstant, rightProperty);
                    }

                    if (node.Right is ConstantExpression rightConstant
                        && node.Left is MemberExpression leftProperty)
                    {
                        (constantExpression, propExpression) = (rightConstant, leftProperty);
                    }

                    if (propExpression?.Expression is not ParameterExpression lambdaParam) return node;
                    
                    var matchParam = Expression.Parameter(matchQueryDescriptorType, "m");

                    var callFieldMethod = Expression.Call(matchParam,
                        nameof(MatchQueryDescriptor<object>.Field),
                        new[] {propExpression.Type},
                        Expression.Lambda(propExpression, lambdaParam)
                    );

                    var callMatchQueryMethod =
                        Expression.Call(callFieldMethod, matchQueryMethod, constantExpression);

                    var callMatchMethod = Expression.Call(queryParam, matchMethod,
                        Expression.Lambda(callMatchQueryMethod, matchParam));

                    methodCall = callMatchMethod;
                }

                if (methodCall != null)
                {
                    var lambdaExpr = Expression.Lambda(methodCall, queryParam);
                    var lambda = lambdaExpr.Compile();

                    SearchDescriptorObject = queryMethod?.Invoke(SearchDescriptorObject, new object[] {lambda});
                }

                return node;
            }

            return base.VisitBinary(node);
        }
    }
}