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
using System.Net.Http;

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
            using (var http = new HttpClient())
            {
                Task m = Task.Run(async () =>
                {
                    string topMoviesUrl = $"http://{baseUrl}/imdb/api/top-movies";
                    var response = await http.GetAsync(topMoviesUrl);
                    MoviesRates = await response.Content.ReadAsAsync<ProfileRate[]>();
                });
                Task s = Task.Run(async () =>
                {
                    string topMoviesUrl = $"http://{baseUrl}/imdb/api/top-stars";
                    var response = await http.GetAsync(topMoviesUrl);
                    StarsRates = await response.Content.ReadAsAsync<ProfileRate[]>();
                });
                await Task.WhenAll(m, s);
            }
            string url = $"http://{baseUrl}/imdb";
            var hubConnection = new HubConnection(url);
            _hubProxy = hubConnection.CreateHubProxy(Constants.HubName);
            _hubProxy.On<TwittData >("BroadcastLikeMovie", LikeMovie);
            _hubProxy.On<TwittData>("BroadcastLikeStar", LikeStar);
            _hubProxy.On<ChangedData>("BroadcastChanged", Changed);
            _hubProxy.On<string>("BroadcastParserError", ParserError);
            await hubConnection.Start();
        }

        public void LikeMovie(TwittData  movie)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Events.Insert(0, movie);
            });
        }

        public void LikeStar(TwittData star)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Events.Insert(0, star);
            });
        }

        public void Changed(ChangedData data)
        {
            switch (data.Kind)
            {
                case ImdbType.Unknown:
                    break;
                case ImdbType.Movie:
                    MoviesRates = data.Items;
                    break;
                case ImdbType.Star:
                    StarsRates = data.Items;
                    break;
                default:
                    break;
            }
        }

        #region ParserError

        public void ParserError(string url)
        {

        }

        #endregion // ParserError
    }
}
