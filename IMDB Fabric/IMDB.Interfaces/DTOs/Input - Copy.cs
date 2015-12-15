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
    public class ChangedData 
    {
        [DataMember]
        public ImdbType Kind { get; set; }
        [DataMember]
        public ProfileRate[] Items { get; set; }
    }
}
