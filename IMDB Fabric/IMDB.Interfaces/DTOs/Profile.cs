﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace IMDB.Interfaces
{
    [DebuggerDisplay("{Name}: {ImageUrl}")]
    [DataContract]
    public class Profile
    {
        public Profile(string name, string imageUrl)
        {
            Name = name;
            ImageUrl = imageUrl;
        }

        [DataMember]
        public string Name { get; private set; }
        [DataMember]
        public string ImageUrl { get; private set; }
    }
}
