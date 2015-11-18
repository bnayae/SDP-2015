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
    public class ImdbFaults : StatelessActor, IImdbFaults
    {
        public Task ReportParsingError(string url)
        {
            var e = GetEvent<IImdbFaultEvents>();

            // Clients can listen for the event 
            // (events shouldn't be used for Actor's internal communication)
            // when ready Rx 3 will be the publication mechanism
            e.ParserError(url);
            return Task.CompletedTask;
        }
    }
}
