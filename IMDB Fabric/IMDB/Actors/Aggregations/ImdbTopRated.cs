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
    public class ImdbTopRated : StatefulActor<ProfileRate[]>, IImdbTopRated
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

        public Task OferCandidateAsync(ProfileRate profile)
        {
            ProfileRate existing = _topItems.FirstOrDefault(m => m.Name == profile.Name);
            if (existing != null)
            {
                existing.Count++; // increment the item which already in the state
                Publish();

                return Task.FromResult(0);
            }

            ProfileRate barierProfile = _topItems
                                .OrderBy(m => m.Count)
                                .FirstOrDefault();
            int barier = barierProfile?.Count ?? 0;

            if (_topItems.Count == LIMIT && profile.Count <= barier)
                return Task.FromResult(0);

            _topItems.Add(profile);

            if (_topItems.Count > LIMIT)
            {
                var removeCandidate = 
                    _topItems.OrderBy(m => m.Count)
                             .First();
                _topItems.Remove(removeCandidate);
            }

            State = _topItems.ToArray();

            Publish();

            return Task.FromResult(0);
        }

        private void Publish()
        {
            IImdbTopRatedEvents e = GetEvent<IImdbTopRatedEvents>();
            var items = State.OrderByDescending(m => m.Count).ToArray();
            var data = new ChangedData { Kind = _type, Items = items };
            e.Changed(data);
        }
    }
}
