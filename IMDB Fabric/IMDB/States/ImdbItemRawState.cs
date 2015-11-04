using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using System.Runtime.Serialization;

namespace IMDB
{
    /// <summary>
    /// Represent IMDB's Movie or Star
    /// </summary>
    [DataContract]
    public class ImdbItemRawState
    {
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public ImdbType Type { get; set; }
        [DataMember]
        public string ImageUrl { get; set; }
        [DataMember]
        public DateTime Date { get; set; }
    }
}
