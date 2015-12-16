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
    public class ImdbHub : StatelessActor, IImdbHub
    {
        public async Task SendStarAsync(TwittData data)
        {
            ActorEventSource.Current.ActorMessage(this, data.Name);

            // Clients can listen for the event 
            // (events shouldn't be used for Actor's internal communication)
            // when ready Rx 3 will be the publication mechanism
            IImdbEvents e = GetEvent<IImdbEvents>();
            e.LikeStar(data);

            #region Log

            var logId = Constants.Singleton;
            var logProxy = ActorProxy.Create<IImdbFaults>(logId);
            await logProxy.Report($"Raise Star Event: {data.Name}");

            #endregion // Log
        }

        public async Task SendMovieAsync(TwittData  data)
        {
            IImdbEvents e = GetEvent<IImdbEvents>();

            // Clients can listen for the event 
            // (events shouldn't be used for Actor's internal communication)
            // when ready Rx 3 will be the publication mechanism
            e.LikeMovie(data);

            #region Log

            var logId = Constants.Singleton;
            var logProxy = ActorProxy.Create<IImdbFaults>(logId);
            await logProxy.Report($"Raise Movie Event: {data.Name}");

            #endregion // Log
        }
    }
}
