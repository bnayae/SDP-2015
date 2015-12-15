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
    public class TwittData : Profile
    {
        [Obsolete("for json serialization", true)]
        public TwittData(): base("", "") { }

        public TwittData(
            Profile data,
            Profile sender = null)
            : base(data.Name, data.ImageUrl)
        {
            Sender = sender;
        }

        [DataMember]
        public Profile Sender { get; private set; }
    }
}
