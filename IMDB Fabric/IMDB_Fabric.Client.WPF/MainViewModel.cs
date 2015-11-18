using IMDB.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using IMDB;
using System.Collections.ObjectModel;
using Microsoft.ServiceFabric.Actors;

namespace IMDB_Fabric.Client.WPF
{
    public class MainViewModel : IImdbEvents, IImdbTopRatedEvents, IImdbFaultEvents, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Properties
        public ObservableCollection<Profile> Events { get; } = new ObservableCollection<Profile>();

        private ProfileRate[] _moviesRates;

        public ProfileRate[] MoviesRates
        {
            get { return _moviesRates; }
            set
            {
                _moviesRates = value;
                RaisePropertyChanged();
            }
        }

        private ProfileRate[] _starsRates;

        public ProfileRate[] StarsRates
        {
            get { return _starsRates; }
            set
            {
                _starsRates = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public MainViewModel()
        {
            Task t = Initialize();
        }

        private async Task Initialize()
        {
            var hubId = new ActorId("PUBLISH");// Kind of topic;
            var movieId = new ActorId(ImdbType.Movie.ToString());// Kind of topic;
            var starId = new ActorId(ImdbType.Star.ToString());// Kind of topic;
            var proxyHub = ActorProxy.Create<IImdbHub>(hubId, "fabric:/IMDB_Fabric");
            var proxyTopMovie = ActorProxy.Create<IImdbTopRated>(movieId, "fabric:/IMDB_Fabric");
            var proxyTopStar = ActorProxy.Create<IImdbTopRated>(starId, "fabric:/IMDB_Fabric");
            var proxyFaults = ActorProxy.Create<IImdbFaults>(hubId, "fabric:/IMDB_Fabric");
            while (true)
            {
                try
                {
                    await proxyHub.SubscribeAsync<IImdbEvents>(this);
                    await proxyFaults.SubscribeAsync<IImdbFaultEvents>(this);
                    await proxyTopMovie.SubscribeAsync<IImdbTopRatedEvents>(this);
                    await proxyTopStar.SubscribeAsync<IImdbTopRatedEvents>(this);
                    Console.WriteLine("Ready");
                    break;
                }
                catch (Exception)
                {
                    var actorRef = proxyHub.GetActorReference();
                    var uri = actorRef?.ServiceUri;
                    Console.WriteLine($"Wait for: {uri}");
                    await Task.Delay(2000);
                }
            }
        }

        public void LikeMovie(Movie movie)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Events.Add(movie);
            });
        }

        public void LikeStar(Star star)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Events.Add(star);
            });
        }

        public void Changed(ImdbType type, ProfileRate[] items)
        {
            switch (type)
            {
                case ImdbType.Unknown:
                    break;
                case ImdbType.Movie:
                    MoviesRates = items;
                    break;
                case ImdbType.Star:
                    StarsRates = items;
                    break;
                default:
                    break;
            }
        }

        public void ParserError(string url)
        {
            
        }
    }
}
