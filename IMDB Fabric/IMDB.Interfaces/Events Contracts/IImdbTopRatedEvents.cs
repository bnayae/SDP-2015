﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace IMDB.Interfaces
{
    public interface IImdbTopRatedEvents : IActorEvents
    {
        void Changed(ChangedData data);
    }
}
