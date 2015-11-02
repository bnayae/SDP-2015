using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using System.Runtime.Serialization;

namespace IMDB.Interfaces
{
    [DataContract]
    public abstract class ImdbItemBase
    {
        public ImdbItemBase(string name, string imageUrl)
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
