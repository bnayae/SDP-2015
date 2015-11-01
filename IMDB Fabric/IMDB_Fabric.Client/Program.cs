using IMDB.Interfaces;
using Microsoft.ServiceFabric.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDB_Fabric.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Task t = InitializeAsync();
            Console.ReadKey();
        }

        private static async Task InitializeAsync()
        {
            var id = new ActorId("PUBLISH");// Kind of topic;
            var proxy = ActorProxy.Create<IOutputPublisher>(id, "fabric:/IMDB_Fabric");

            var subscriber = new MovieEvent();
            await proxy.SubscribeAsync<IMovieEvent>(subscriber);
        }
    }
}
