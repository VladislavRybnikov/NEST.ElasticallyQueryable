using System;
using System.Linq;
using System.Linq.Expressions;
using Nest;
using NEST.ElasticallyQueryable.Internal;
using NEST.ElasticallyQueryable.Internals.Visitors.Base;

namespace NEST.ElasticallyQueryable.Internals.Visitors
{
    public class ElasticQueryVisitor<T> : ElasticVisitor
    {
        public ElasticQueryVisitor() : base(new InternalSearchDescriptorAccessor())
        {
        }

        public ElasticQueryVisitor(ISearchDescriptorAccessor accessor) : base(accessor)
        {
            
        }

        protected ElasticVisitor ResolveVisitor(string methodName) =>
            methodName switch
            {
                nameof(Queryable.Skip) => new SkipVisitor(this),
                nameof(Queryable.Take) => new TakeVisitor(this),
                nameof(Queryable.Where) => new WhereVisitor<T>(this),
                _ => throw new NotImplementedException($"{methodName} is not supported")
            };

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var arg = node.Arguments.FirstOrDefault(e => !IsQuery(e.Type));
            var visitor = ResolveVisitor(node.Method.Name);
            visitor.Visit(arg);
            return base.VisitMethodCall(node);
        }

        private class InternalSearchDescriptorAccessor : ISearchDescriptorAccessor
        {
            public Type SearchDescriptorType { get; }
            public object SearchDescriptorObject { get; }

            public InternalSearchDescriptorAccessor()
            {
                SearchDescriptorType = typeof(SearchDescriptor<>).MakeGenericType(typeof(T));
                SearchDescriptorObject = Activator.CreateInstance(SearchDescriptorType);
            }
        }
    }
}