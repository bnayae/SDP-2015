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
        public Task LoadAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Doing Work");
            return Task.CompletedTask;
        }
    }
}
