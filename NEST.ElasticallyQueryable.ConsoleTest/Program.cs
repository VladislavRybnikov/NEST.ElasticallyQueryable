using System;
using System.Linq;
using Moq;
using Nest;

namespace NEST.ElasticallyQueryable.ConsoleTest
{
    public class User
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var client = Mock.Of<IElasticClient>();

            client.Search<User>(s 
                => s.Query(q => q.Match(m 
                    => m.Field(f => f.Name).Query("Vlad"))));
            
            client
                .QueryElastically<User>()
                .Where(u => u.Name == "Vlad")
                .SearchAsync();
            
            Console.WriteLine("Hello World!");
        }
    }
}