﻿using Microsoft.ServiceFabric.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDB.Interfaces
{
    public static class Constants
    {
        public const string HubName = "imdb";
        public static readonly ActorId Singleton =
            new ActorId("Singleton");
    }
}
