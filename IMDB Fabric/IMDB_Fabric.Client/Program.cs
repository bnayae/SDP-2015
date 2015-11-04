using IMDB;
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
            var hubId = new ActorId("PUBLISH");// Kind of topic;
            var movieId = new ActorId(ImdbType.Movie.ToString());// Kind of topic;
            var starId = new ActorId(ImdbType.Star.ToString());// Kind of topic;
            var proxyHub = ActorProxy.Create<IImdbHub>(hubId, "fabric:/IMDB_Fabric");
            var proxyTopMovie = ActorProxy.Create<IImdbTopRated>(movieId, "fabric:/IMDB_Fabric");
            var proxyTopStar = ActorProxy.Create<IImdbTopRated>(starId, "fabric:/IMDB_Fabric");
            var proxyFaults = ActorProxy.Create<IImdbFaults>(hubId, "fabric:/IMDB_Fabric");
            while (true)
            {
                try
                {
                    var subscriber = new ImdbEvents();
                    await proxyHub.SubscribeAsync<IImdbEvents>(subscriber);
                    await proxyFaults.SubscribeAsync<IImdbFaultEvents>(subscriber);
                    await proxyTopMovie.SubscribeAsync<IImdbTopRatedEvents>(subscriber);
                    await proxyTopStar.SubscribeAsync<IImdbTopRatedEvents>(subscriber);
                    Console.WriteLine("Ready");
                    break;
                }
                catch (Exception)
                {
                    var actorRef = proxyHub.GetActorReference();
                    var uri = actorRef?.ServiceUri;
                    Console.WriteLine($"Wait for: {uri}");
                    await Task.Delay(2000);
                }
            }
        }
    }
}
