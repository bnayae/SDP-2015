using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace IMDB.Interfaces
{
    [DebuggerDisplay("{Name}: {Count}")]
    [DataContract]
    public class ProfileRate: 
        Profile
    {
        [Obsolete("For serialization", true)]
        public ProfileRate(): base(null, null)
        {
        }

        public ProfileRate(Profile profile, int count)
            :base(profile.Name, profile.ImageUrl)
        {
            Count = count;
        }

        [DataMember]
        public int Count { get; set; }
    }
}
