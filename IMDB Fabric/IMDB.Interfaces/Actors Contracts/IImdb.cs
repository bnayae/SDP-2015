using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace IMDB.Interfaces
{
    /// <summary>
    /// IMDB processing, Actor Id should be URL of Star or Movie
    /// meant to be instantiate with instance per URL (Actor Id)
    /// </summary>
    public interface IImdb : IActor 
    {
        Task<bool> TryProcess(Input data);
    }
}
