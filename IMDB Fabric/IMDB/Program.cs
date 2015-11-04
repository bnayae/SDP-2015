using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using Microsoft.ServiceFabric.Actors;
using IMDB.Interfaces;

namespace IMDB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                using (FabricRuntime fabricRuntime = FabricRuntime.Create())
                {
                    fabricRuntime.RegisterActor(typeof(Imdb));
                    fabricRuntime.RegisterActor(typeof(ImdbHub));
                    fabricRuntime.RegisterActor(typeof(ImdbCounter));
                    fabricRuntime.RegisterActor(typeof(ImdbTopRated));
                    fabricRuntime.RegisterActor(typeof(ImdbFaults));

                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e);
                throw;
            }
        }
    }
}
