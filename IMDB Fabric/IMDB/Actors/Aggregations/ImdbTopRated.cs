using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IMDB.Interfaces;
using Microsoft.ServiceFabric;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Communication;

namespace IMDB
{
    public class ImdbTopRated : 
        StatefulActor<ProfileRate[]>, IImdbTopRated
    {
        private const int LIMIT = 3;
        private ImdbType _type;

        private List<ProfileRate> _topItems;
        protected override Task OnActivateAsync()
        {
            if (!Enum.TryParse(Id.ToString(), out _type))
                throw new NotSupportedException($"Id expected to represent {nameof(ImdbType)}");

            _topItems = new List<ProfileRate>(State ?? new ProfileRate[0]);
            return base.OnActivateAsync();
        }

        public async Task OferCandidateAsync(ProfileRate profile)
        {
            ProfileRate existing = _topItems.FirstOrDefault(m => m.Name == profile.Name);
            if (existing != null)
            {
                existing.Count++; // increment the item which already in the state
                Publish();

            }

            ProfileRate barierProfile = _topItems
                                .OrderBy(m => m.Count)
                                .FirstOrDefault();
            int barier = barierProfile?.Count ?? 0;

            if (_topItems.Count == LIMIT && profile.Count <= barier)
                return;

            _topItems.Add(profile);

            if (_topItems.Count > LIMIT)
            {
                var removeCandidate = 
                    _topItems.OrderBy(m => m.Count)
                             .First();
                _topItems.Remove(removeCandidate);
            }

            State = _topItems.OrderByDescending(m => m.Count)
                             .ToArray();

            Publish();

            #region Log

            var logId = Constants.Singleton;
            var logProxy = ActorProxy.Create<IImdbFaults>(logId);
            await logProxy.Report($"Top {_type}");

            #endregion // Log
        }

        public Task<ProfileRate[]> Get()
        {
            return Task.FromResult(State);
        }

        private void Publish()
        {
            IImdbTopRatedEvents e = GetEvent<IImdbTopRatedEvents>();
            var items = State;
            var data = new ChangedData { Kind = _type, Items = items };
            e.Changed(data);
        }
    }
}
