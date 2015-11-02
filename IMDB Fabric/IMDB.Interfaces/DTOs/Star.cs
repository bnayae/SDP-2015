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
    public class Star : ImdbItemBase
    {
        public Star(string name, DateTime birthdate, string imageUrl)
            :base(name, imageUrl)
        {
            Birthdate = birthdate;
        }
        [DataMember]
        public DateTime Birthdate { get; private set; }
    }
}
