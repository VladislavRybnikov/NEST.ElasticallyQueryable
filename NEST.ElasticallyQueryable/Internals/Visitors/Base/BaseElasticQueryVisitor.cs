using System;
using System.Linq;
using System.Linq.Expressions;
using NEST.ElasticallyQueryable.Internal;

namespace NEST.ElasticallyQueryable.Internals.Visitors.Base
{
    public abstract class ElasticVisitor : ExpressionVisitor, ISearchDescriptorAccessor
    {
        public Type SearchDescriptorType { get; }
        public object SearchDescriptorObject { get; protected set; }
        
        protected ElasticVisitor(ISearchDescriptorAccessor accessor)
        {
            SearchDescriptorType = accessor.SearchDescriptorType;
            SearchDescriptorObject = accessor.SearchDescriptorObject;
        }

        public bool IsQuery(Type type) 
            => type.IsGenericType
               && (type.GetGenericTypeDefinition() == typeof(ElasticQuery<>) 
                   || type.GetGenericTypeDefinition() == typeof(IQueryable<>));
    }
}