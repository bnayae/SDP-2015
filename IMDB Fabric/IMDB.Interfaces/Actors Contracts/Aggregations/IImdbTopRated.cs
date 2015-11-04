using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace IMDB.Interfaces
{
    /// <summary>
    /// Actor Id expected to represent ImdbType's name
    /// </summary>
    public interface IImdbTopRated : IActor, IActorEventPublisher<IImdbTopRatedEvents>
    {
        Task OferCandidateAsync(ProfileRate profile);
    }
}

