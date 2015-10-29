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
            var proxy = ActorProxy.Create<ITweetLoader>(ActorId.NewId(), "fabric:/IMDB_Fabric");

            proxy.LoadAsync();

            Console.ReadKey();
        }
    }
}
