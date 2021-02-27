using System;
using System.Linq.Expressions;
using Nest;
using NEST.ElasticallyQueryable.Internals.Visitors;

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

            var visitor = new ElasticQueryVisitor<T>();
            visitor.Visit(expression);

            return visitor.SearchDescriptorObject;
        }
    }
}