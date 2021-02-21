using System;
using System.Linq;
using System.Linq.Expressions;
using Nest;

namespace NEST.ElasticallyQueryable
{
    public class ElasticQueryContext<T>
    {
        internal IElasticClient Client { get; }

        public ElasticQueryContext(IElasticClient client)
        {
            Client = client;
        }

        public object Execute(Expression expression, bool isEnumerable = false)
        {
            if (!typeof(T).IsClass) throw new InvalidOperationException();
            
            var searchDescriptorType = typeof(SearchDescriptor<>).MakeGenericType(typeof(T));
            
            var searchDescriptor = Activator.CreateInstance(searchDescriptorType);

            foreach (var arg in (expression as MethodCallExpression)?.Arguments ?? Enumerable.Empty<Expression>())
            {
                Visit(arg, searchDescriptor, searchDescriptorType);
            }

            return searchDescriptor as ISearchRequest;
        }

        public void Visit(Expression expression, object searchDescriptor, Type searchDescriptorType)
        {
            if(expression.NodeType == ExpressionType.Constant) return;

            if (expression is UnaryExpression unary)
            {
                Visit(unary.Operand, searchDescriptor, searchDescriptorType);
            }
            
            if (expression is LambdaExpression lambdaExpression)
            {
                if (lambdaExpression.Body is BinaryExpression binaryExpression)
                {
                    var matchQueryDescriptorType = (typeof(MatchQueryDescriptor<>)).MakeGenericType(typeof(T));
                    
                    var queryMethod = searchDescriptorType.GetMethod(nameof(SearchDescriptor<object>.Query));
                    var matchQueryMethod =
                        matchQueryDescriptorType.GetMethod(nameof(MatchQueryDescriptor<object>.Query));
                    
                    MethodCallExpression methodCall = null;
                    
                    var queryContainerType = typeof(QueryContainerDescriptor<>).MakeGenericType(typeof(T));
                    
                    var queryParam = Expression.Parameter(queryContainerType, "q");
                    
                    if (binaryExpression.NodeType == ExpressionType.Equal)
                    {
                        var matchMethod = typeof(QueryContainerDescriptor<>).MakeGenericType(typeof(T))
                            .GetMethod(nameof(QueryContainerDescriptor<object>.Match));
                        
                        ConstantExpression constantExpression = null;
                        Expression propExpression = null;

                        if (binaryExpression.Left is ConstantExpression left) 
                            (constantExpression, propExpression) = (left, binaryExpression.Right);
                        if (binaryExpression.Right is ConstantExpression right) 
                            (constantExpression, propExpression) = (right, binaryExpression.Left);

                        var matchParam = Expression.Parameter(matchQueryDescriptorType, "m");

                        var callFieldMethod = Expression.Call(matchParam, 
                            nameof(MatchQueryDescriptor<object>.Field), 
                            new Type[]{propExpression.Type}, Expression
                                .Lambda(propExpression, lambdaExpression.Parameters[0]));
                        
                        var callMatchQueryMethod = Expression.Call(callFieldMethod, matchQueryMethod, constantExpression);

                        var callMatchMethod = Expression.Call(queryParam, matchMethod, 
                            Expression.Lambda(callMatchQueryMethod, matchParam));
                        
                        methodCall = callMatchMethod;

                    }

                    if (methodCall != null)
                    {
                        var lambdaExpr = Expression.Lambda(methodCall, queryParam);
                        var lambda = lambdaExpr.Compile();

                        queryMethod?.Invoke(searchDescriptor, new object[] {lambda});
                    }
                }
            }
        }
    }


}