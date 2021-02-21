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
        public static IQueryable<T> QueryElastically<T>(this IElasticClient elasticClient)
            where T : class
        {
            var context = new ElasticQueryContext<T>(elasticClient);
            return new ElasticQuery<T>(context);
        }

        public static Task<ISearchResponse<T>> SearchAsync<T>(this IQueryable<T> queryable)
            where T : class
        {
            if (queryable is ElasticQuery<T> query)
            {
                return query.SearchInternalAsync<T>();
            }

            throw new InvalidOperationException();
        }
    }
}