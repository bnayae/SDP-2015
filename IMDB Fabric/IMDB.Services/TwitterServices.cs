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

namespace IMDB.Services
{
    public class TwitterServices : StatelessService
    {
        protected override ICommunicationListener CreateCommunicationListener()
        {
            // TODO: Replace this with an ICommunicationListener implementation if your service needs to handle user requests.
            return base.CreateCommunicationListener();
        }
        
        protected override async Task RunAsync(
            CancellationToken cancellationToken)
        {
            var auth = await AuthorizeAsync();
            var twitterCtx = new TwitterContext(auth);

            var stream = from srm in twitterCtx.Streaming
                    where srm.Type == StreamingType.Filter &&
                           srm.Track == "sdpf"
                    select srm;
            await stream.StartAsync(async context =>
            {
                if (context.Entity == null && context.EntityType != StreamEntityType.Status)
                    return;
                var status = (Status)context.Entity;
                var url = status.Entities.UrlEntities.FirstOrDefault()?.ExpandedUrl;

                var input = new Input
                    {
                        Url = url,
                        UserImageUrl = status.User.ProfileImageUrl,
                        UserName = status.User.Name
                    };

                var id = ActorId.NewId();
                var proxy = ActorProxy.Create<IImdb>(id, "fabric:/IMDB_Fabric");
                await proxy.Process(input);
            });

            await Task.Delay(TimeSpan.MinValue, cancellationToken);
        }

        private async Task<IAuthorizer> AuthorizeAsync()
        {
            ServiceEventSource.Current.ServiceMessage(this, "Twitter: Authorizing");
            try
            {
                string tmp = CloudConfigurationManager.GetSetting("IsLocal");
                bool isLocal;
                string consumerKey, consumerSecret, accessToken, accessTokenSecret;
                if (bool.TryParse(tmp, out isLocal) && isLocal)
                {
                    consumerKey = Environment.GetEnvironmentVariable("TweeterConsumerKey");
                    consumerSecret = Environment.GetEnvironmentVariable("TweeterConsumerSecret");
                    accessToken = Environment.GetEnvironmentVariable("TwitterAccessToken");
                    accessTokenSecret = Environment.GetEnvironmentVariable("TwitterAccessTokenSecret");
                }
                else
                {
                    consumerKey = CloudConfigurationManager.GetSetting("TwittererConsumerKey");
                    consumerSecret = CloudConfigurationManager.GetSetting("TwittererConsumerSecret");
                    accessToken = CloudConfigurationManager.GetSetting("TwitterAccessToken");
                    accessTokenSecret = CloudConfigurationManager.GetSetting("TwitterAccessTokenSecret");
                }

                var auth = new SingleUserAuthorizer()
                {
                    CredentialStore = new SingleUserInMemoryCredentialStore
                    {
                        ConsumerKey = consumerKey ,
                        ConsumerSecret = consumerSecret,
                        AccessToken = accessToken,
                        AccessTokenSecret = accessTokenSecret
                    }
                };

                await auth.AuthorizeAsync();
                ServiceEventSource.Current.ServiceMessage(this, "Twitter: Authorized");

                return auth;
            }
            #region Exception Handling

            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(ex);
                throw;
            }

            #endregion // Exception Handling
        }
    }
}
