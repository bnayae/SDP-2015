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
    public class Input 
    {
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string UserImageUrl { get; set; }
        [DataMember]
        public string Url { get; set; }
    }
}
