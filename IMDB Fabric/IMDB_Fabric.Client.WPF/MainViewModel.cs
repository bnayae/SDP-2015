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
using Microsoft.AspNet.SignalR.Client;
using System.Configuration;

namespace IMDB_Fabric.Client.WPF
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private IHubProxy _hubProxy;

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
            string baseUrl = ConfigurationManager.AppSettings["base-url"];
            string url = $"http://{baseUrl}/imdb";
            var hubConnection = new HubConnection(url);
            _hubProxy = hubConnection.CreateHubProxy(Constants.HubName);
            _hubProxy.On<Movie>("BroadcastLikeMovie", LikeMovie);
            _hubProxy.On<Star>("BroadcastLikeStar", LikeStar);
            _hubProxy.On<ImdbType, ProfileRate[]>("BroadcastChanged", Changed);
            _hubProxy.On<string>("BroadcastParserError",ParserError);
            await hubConnection.Start();
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
