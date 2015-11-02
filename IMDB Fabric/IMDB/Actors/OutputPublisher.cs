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
    public class OutputPublisher : Actor, IOutputPublisher
    {
        public Task SendStarAsync(Star data)
        {
            ActorEventSource.Current.ActorMessage(this, data.Name);

            IMovieEvent e = GetEvent<IMovieEvent>();
            e.LikeStar(data);
            return Task.CompletedTask;
        }

        public Task SendMovieAsync(Movie data)
        {
            IMovieEvent e = GetEvent<IMovieEvent>();
            
            ActorEventSource.Current.ActorMessage(this, $"Publish ", data.Name);
            e.LikeMovie(data);
            return Task.CompletedTask;
        }
    }
}
