using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IMDB.Interfaces;
using Microsoft.ServiceFabric;
using Microsoft.ServiceFabric.Actors;

namespace IMDB
{
    public class TweetLoader : Actor, ITweetLoader
    {
        public override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Activating");
            return base.OnActivateAsync();
        }
        public async Task LoadAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Doing Work");

            var id = new ActorId(1);// ActorId.NewId();
            var proxy = ActorProxy.Create<IImdb>(id, "fabric:/IMDB_Fabric");

            await proxy.Process("not real tweet");
        }
    }
}
