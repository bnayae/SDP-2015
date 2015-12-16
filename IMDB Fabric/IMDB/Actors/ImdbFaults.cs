using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IMDB.Interfaces;
using Microsoft.ServiceFabric;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Communication;
using System.Diagnostics.Tracing;

namespace IMDB
{
    public class ImdbFaults : StatefulActor<LogsState>, IImdbFaults
    {
        protected async override Task OnLoadStateAsync()
        {
            await base.OnLoadStateAsync();
            if (State == null)
                State = new LogsState();
        }
        public Task Report(string message)
        {
            var msg = new LogItem { Title = "Log", Level = EventLevel.Informational, Message = message };
            State.Logs.Add(msg);
            return Task.FromResult(1);
        }
        public Task ReportError(string error)
        {
            var message = new LogItem { Title = "Error", Level = EventLevel.Error, Message = error };
            State.Logs.Add(message);
            return Task.FromResult(1);
        }

        public Task ReportParsingError(string url)
        {
            var message = new LogItem { Title = "ParsingError", Level = EventLevel.Warning, Message = url };
            State.Logs.Add(message);

            var e = GetEvent<IImdbFaultEvents>();

            // Clients can listen for the event 
            // (events shouldn't be used for Actor's internal communication)
            // when ready Rx 3 will be the publication mechanism
            e.ParserError(url);
            return Task.FromResult(0);
        }

        public Task<LogItem[]> GetAsync()
        {
            var items = State.Logs.ToArray();
            return Task.FromResult(items);
        }
    }
}
