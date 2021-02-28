using System;
using System.Linq.Expressions;
using System.Reflection;
using Nest;
using NEST.ElasticallyQueryable.Internal;
using NEST.ElasticallyQueryable.Internals.Visitors.Base;

namespace NEST.ElasticallyQueryable.Internals.Visitors
{
    public class WhereVisitor<T> : ElasticVisitor
    {
        private static class MatchQueryDescriptorInfo
        {
            public static Type Type 
                => typeof(MatchQueryDescriptor<>).MakeGenericType(typeof(T));
            
            public static MethodInfo QueryMethod 
                => Type.GetMethod(nameof(MatchQueryDescriptor<object>.Query));

            public static string FieldMethodName => nameof(MatchQueryDescriptor<object>.Field);
        }

        private static class QueryContainerDescriptorInfo
        {
            public static Type Type => typeof(QueryContainerDescriptor<>).MakeGenericType(typeof(T));
            
            public static MethodInfo MatchMethod => Type.GetMethod(nameof(QueryContainerDescriptor<object>.Match));
        }

        private ConstantExpression _constant;
        private MemberExpression _member;
        
        public WhereVisitor(ISearchDescriptorAccessor accessor) : base(accessor)
        {
            _constant = null;
            _member = null;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _member = node;
            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _constant = node;
            return base.VisitConstant(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            base.VisitBinary(node);
            
            if (node.Type == typeof(bool))
            {
                var queryMethod = SearchDescriptorType.GetMethod(nameof(SearchDescriptor<object>.Query));

                var queryParam = Expression.Parameter(QueryContainerDescriptorInfo.Type, "q");
                var methodCall = node.NodeType switch
                {
                    ExpressionType.Equal => VisitEquality(queryParam),
                    _ => null
                };

                if (methodCall != null)
                {
                    var lambdaExpr = Expression.Lambda(methodCall, queryParam);
                    var lambda = lambdaExpr.Compile();

                    SearchDescriptorObject = queryMethod?.Invoke(SearchDescriptorObject, new object[] {lambda});
                }

                return node;
            }

            return node;
        }

        private MethodCallExpression VisitEquality(ParameterExpression queryParam)
        {
            if (_member?.Expression is not ParameterExpression lambdaParam) return null;
                    
            var matchParam = Expression.Parameter(MatchQueryDescriptorInfo.Type, "m");

            var callFieldMethod = Expression.Call(matchParam,
                MatchQueryDescriptorInfo.FieldMethodName,
                new[] {_member.Type},
                Expression.Lambda(_member, lambdaParam)
            );

            var callMatchQueryMethod =
                Expression.Call(callFieldMethod, MatchQueryDescriptorInfo.QueryMethod, _constant);

            var callMatchMethod = Expression.Call(queryParam, QueryContainerDescriptorInfo.MatchMethod,
                Expression.Lambda(callMatchQueryMethod, matchParam));

            return callMatchMethod;
        }
    }
}