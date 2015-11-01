using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IMDB.Interfaces;
using Microsoft.ServiceFabric;
using Microsoft.ServiceFabric.Actors;
using Microsoft.Azure;
using LinqToTwitter;

namespace IMDB
{
    /// <summary>
    /// Query Twitter for #sdpf and forward the tweet data
    /// </summary>
    public class TwitterProvider : Actor<Modification>, ITwitterProvider
    {
        private ApplicationOnlyAuthorizer _auth;

        public override async Task OnActivateAsync()
        {
            await AuthorizeAsync();

            await base.OnActivateAsync();
        }

        private async Task AuthorizeAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Authorizing");
            try
            {
                string tmp = CloudConfigurationManager.GetSetting("IsLocal");
                bool isLocal;
                _auth = new ApplicationOnlyAuthorizer();
                string consumerKey, consumerSecret;
                if (bool.TryParse(tmp, out isLocal) && isLocal)
                {
                    consumerKey = Environment.GetEnvironmentVariable("TwittererConsumerKey");
                    consumerSecret = Environment.GetEnvironmentVariable("TwittererConsumerSecret");
                }
                else
                {
                    consumerKey = CloudConfigurationManager.GetSetting("TwittererConsumerKey");
                    consumerSecret = CloudConfigurationManager.GetSetting("TwittererConsumerSecret");
                }
                _auth.CredentialStore = new InMemoryCredentialStore
                {
                    ConsumerKey = consumerKey,
                    ConsumerSecret = consumerSecret
                };
                await _auth.AuthorizeAsync();
                ActorEventSource.Current.ActorMessage(this, "Authorized");
            }
            #region Exception Handling

            catch (Exception ex)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(ex);
                throw;
            }

            #endregion // Exception Handling
        }

        public async Task LoadAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Doing Work");

            foreach (Input input in await ParseTwitters())
            {
                var id = ActorId.NewId();
                var proxy = ActorProxy.Create<IImdb>(id, "fabric:/IMDB_Fabric");
                await proxy.Process(input);
            }
        }

        private async Task<IEnumerable<Input>> ParseTwitters()
        {
            var twitterCtx = new TwitterContext(_auth);
            DateTimeOffset lastModification = State.LastModified;

            IQueryable<Search> query =
                from search in twitterCtx.Search
                where search.Type == SearchType.Search &&
                      search.Query == "#sdpf"
                //search.Statuses != null
                select search;
            IEnumerable<Input> response =
                from search in await query.ToListAsync()
                from status in search.Statuses
                    //where status.CreatedAt > lastModification.DateTime.ToUniversalTime()
                    //let text = status.Text
                    //let startAt = status.Text.IndexOf("http")
                    //where startAt > -1
                    //let endAtExists = status.Text.IndexOf(" ", startAt)
                    //let endAt = endAtExists == -1 ? status.Text.Length : endAtExists
                    //let count = endAt - startAt
                    //let url = status.Text.Substring(startAt, count)
                //select status;
            select new Input
            {
                Url = status.Text,// url,
                UserImageUrl = status.User.ProfileImageUrl,
                UserName = status.User.Name ?? status.User.ScreenName
            };

            var reults = response.ToArray();
            if (reults.Any())
                State.LastModified = DateTimeOffset.UtcNow;
            return reults;
        }
    }
}
