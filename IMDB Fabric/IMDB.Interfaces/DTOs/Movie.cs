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
    public class Movie 
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Year { get; set; }
    }
}
