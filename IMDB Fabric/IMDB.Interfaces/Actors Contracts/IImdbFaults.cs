using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace IMDB.Interfaces
{
    /// <summary>
    ///  Make sure to listen for specific actor
    /// </summary>
    public interface IImdbFaults : IActor
        , IActorEventPublisher<IImdbFaultEvents>
    {
        Task ReportParsingError(string url);
    }
}

