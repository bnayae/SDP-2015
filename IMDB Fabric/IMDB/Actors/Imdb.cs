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
    public class Imdb : Actor, IImdb
    {
        public async Task Process(Input data)
        {
            var id = new ActorId("PUBLISH"); // should be singleton so the UI can listen to specific actor (like a topic)
            var actor = ActorProxy.Create<IOutputPublisher>(id);
            await actor.SendMovieAsync(new Movie { Name = data.Url , Year = 1001});
        }
    }
}
