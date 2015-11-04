using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IMDB.Interfaces;
using Microsoft.ServiceFabric;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Communication;

namespace IMDB
{
    public class ImdbHub : Actor, IImdbHub
    {
        public Task SendStarAsync(Star data)
        {
            ActorEventSource.Current.ActorMessage(this, data.Name);

            // Clients can listen for the event 
            // (events shouldn't be used for Actor's internal communication)
            // when ready Rx 3 will be the publication mechanism
            IMovieEvent e = GetEvent<IMovieEvent>();
            e.LikeStar(data);
            return Task.CompletedTask;
        }

        public Task SendMovieAsync(Movie data)
        {
            IMovieEvent e = GetEvent<IMovieEvent>();

            // Clients can listen for the event 
            // (events shouldn't be used for Actor's internal communication)
            // when ready Rx 3 will be the publication mechanism
            ActorEventSource.Current.ActorMessage(this, $"Publish ", data.Name);
            e.LikeMovie(data);
            return Task.CompletedTask;
        }
    }
}
