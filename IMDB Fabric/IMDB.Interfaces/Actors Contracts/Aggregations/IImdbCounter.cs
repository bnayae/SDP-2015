using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace IMDB.Interfaces
{
    public interface IImdbCounter : IActor
    {
        /// <summary>
        /// Increments counter.
        /// Assume that the Actor ID = Star or Movie name
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="profile">The profile.</param>
        /// <returns></returns>
        Task IncrementAsync(ImdbType type, Profile profile);
        [Readonly]
        Task<int> GetCount();
    }
}

