﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using System.Runtime.Serialization;

namespace IMDB.Interfaces
{
    [DataContract]
    public class LogsState
    {
        public LogsState()
        {
            Logs = new List<LogItem>();
        }
        [DataMember]
        public List<LogItem> Logs { get; set; }
    }
}
