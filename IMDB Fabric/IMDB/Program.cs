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
                    fabricRuntime.RegisterActor(typeof(TweetLoader));
                    fabricRuntime.RegisterActor(typeof(TweetReminder));
                    fabricRuntime.RegisterActor(typeof(Imdb));

                    var id = new ActorId("Singleton");
                    var reminderProxy = ActorProxy.Create<ITweetReminder>(id);
                    reminderProxy.Initialize();
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
