using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services;
using LinqToTwitter;
using Microsoft.Azure;
using Microsoft.ServiceFabric.Actors;
using IMDB.Interfaces;
using System.Diagnostics;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.AspNet.SignalR.Client;
using System.Fabric;

namespace IMDB.Services
{
    public class TwitterHubServices : StatelessService,
        IImdbEvents, IImdbTopRatedEvents, IImdbFaultEvents
    {
        private IHubProxy _hubProxy;

        #region CreateServiceInstanceListeners

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
                {
                    new ServiceInstanceListener(
                        initParams =>
                            new OwinCommunicationListener(
                                "imdb",
                                new Startup(),
                                initParams))
                };
        }

        #endregion // CreateServiceInstanceListeners

        #region RunAsync

        protected override async Task RunAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                string url = "http://localhost/imdb";
                var hubConnection = new HubConnection(url);
                _hubProxy = hubConnection.CreateHubProxy(Constants.HubName);
                await hubConnection.Start();

                await Initialize();
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(ex);
                throw;
            }
                         
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }

        #endregion // RunAsync

        #region Initialize

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

        #endregion // Initialize

        #region NotifyAsync

        public async Task NotifyAsync<T>(string method, T item)
        {
            try
            {
                await _hubProxy.Invoke(method, item);

                ServiceEventSource.Current.Message($"Notify: {method}");
            }
            #region Exception Handling

            catch (Exception ex)
            {
                ServiceEventSource.Current.ErrorMessage(ex);
            }

            #endregion // Exception Handling
        }

        #endregion // NotifyAsync

        #region LikeMovie

        public async void LikeMovie(Movie movie)
        {
            await NotifyAsync(nameof(ImdbHub.LikeMovie), movie);
        }

        #endregion // LikeMovie

        #region LikeStar

        public async void LikeStar(Star star)
        {
            await NotifyAsync(nameof(ImdbHub.LikeStar), star);
        }

        #endregion // LikeStar

        #region Changed

        public async void Changed(ChangedData data)
        {
            await NotifyAsync(nameof(ImdbHub.Changed), data);
        }

        #endregion // Changed

        #region ParserError

        public async void ParserError(string url)
        {
            await NotifyAsync(nameof(ImdbHub.ParserError), url);
        }

        #endregion // ParserError
    }
}
