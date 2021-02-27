using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nest;

namespace NEST.ElasticallyQueryable.ConsoleTest
{
    public class User
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class FakeElasticClient
    {
        
    }

    class Program
    {
        static void Main(string[] args)
        {
            
            var q1 = NestSearch();
            var q2 = ElasticallyQueryableSearch();
            
            Console.WriteLine("Hello World!");
        }

        private static ISearchRequest NestSearch()
        {
            var searchQuery = new SearchDescriptor<User>();
            
            var clientMock = new Mock<IElasticClient>();

            clientMock.Setup(m => m
                    .Search(It.IsAny<Func<SearchDescriptor<User>, ISearchRequest>>()))
                    .Returns<Func<SearchDescriptor<User>, ISearchRequest>>(
                    f 
                        =>
                    {
                        f(searchQuery);
                        return null;
                    }); 
            
            var client = clientMock.Object;
            client.Search<User>(s 
                => s
                    .From(5)
                    .Size(10)
                    .Query(q => q
                        .Match(m 
                            => m.Field(f => f.Name).Query("Vlad"))));

            return searchQuery;
        }
        
        private static ISearchRequest ElasticallyQueryableSearch()
        {
            var searchQuery = new SearchDescriptor<User>();
            
            var clientMock = new Mock<IElasticClient>();

            clientMock.Setup(m => m
                    .Search(It.IsAny<Func<SearchDescriptor<User>, ISearchRequest>>()))
                .Returns<Func<SearchDescriptor<User>, ISearchRequest>>(
                    f
                    =>
                    {
                        f(searchQuery);
                        return null;
                    }); 
            
            var client = clientMock.Object;
            
            client
                .ElasticallyQuery<User>()
                .Where(u => u.Name == "Vlad")
                .Skip(5)
                .Take(10)
                .Search(s =>
                {
                    searchQuery = s;
                    return s;
                });

            return searchQuery;
        }
    }
}