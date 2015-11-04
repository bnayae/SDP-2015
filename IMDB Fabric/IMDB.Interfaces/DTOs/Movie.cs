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
    public class Movie: Profile
    {
        public Movie(
            string name, 
            int year, 
            string imageUrl, 
            Profile sender = null)
            :base(name, imageUrl)
        {
            Year = year;
            Sender = sender;
        }

        [DataMember]
        public Profile Sender { get; private set; }

        [DataMember]
        public int Year { get; private set; }
    }
}
