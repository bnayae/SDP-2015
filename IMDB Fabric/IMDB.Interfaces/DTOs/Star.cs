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
    public class Star : Profile
    {
        public Star(
            string name, 
            DateTime birthdate, 
            string imageUrl,
            Profile sender = null)
            : base(name, imageUrl)
        {
            Birthdate = birthdate;
            Sender = sender;
        }

        [DataMember]
        public Profile Sender { get; private set; }

        [DataMember]
        public DateTime Birthdate { get; private set; }
    }
}
