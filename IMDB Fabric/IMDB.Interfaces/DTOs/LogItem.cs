using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using System.Runtime.Serialization;
using System.Diagnostics.Tracing;

namespace IMDB.Interfaces
{
    [DataContract]
    public class LogItem
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public EventLevel Level { get; set; }
        [DataMember]
        public string Message { get; set; }

        public override string ToString()
        {
            return $"{Title} ({Level}): {Message}";
        }
    }
}
