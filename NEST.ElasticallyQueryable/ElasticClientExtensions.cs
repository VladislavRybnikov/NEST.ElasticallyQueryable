using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Nest;

namespace NEST.ElasticallyQueryable
{
    public static class ElasticClientExtensions
    {
        public static IQueryable<T> ElasticallyQuery<T>(this IElasticClient elasticClient)
            where T : class
        {
            var context = new ElasticQueryContext<T>(elasticClient);
            return new ElasticQuery<T>(context);
        }

        public static Task<ISearchResponse<T>> SearchAsync<T>(this IQueryable<T> queryable, 
            Func<SearchDescriptor<T>, ISearchRequest> searchFunc = null) 
            where T : class
        {
            if (queryable is ElasticQuery<T> query)
            {
                return query.SearchInternalAsync(searchFunc);
            }

            throw new InvalidOperationException();
        }
        
        public static ISearchResponse<T> Search<T>(this IQueryable<T> queryable, 
            Func<SearchDescriptor<T>, ISearchRequest> searchFunc = null) 
            where T : class
        {
            if (queryable is ElasticQuery<T> query)
            {
                return query.SearchInternal(searchFunc);
            }

            throw new InvalidOperationException();
        }
    }
}