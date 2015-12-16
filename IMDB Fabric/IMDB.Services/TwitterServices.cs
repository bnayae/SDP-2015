﻿using System;
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

namespace IMDB.Services
{
    public class TwitterServices : StatelessService
    {
        #region RunAsync

        protected override async Task RunAsync(
            CancellationToken cancellationToken)
        {
            var auth = await AuthorizeAsync();
            var twitterCtx = new TwitterContext(auth);

            await ClearOldTwits(twitterCtx);

            var stream = from srm in twitterCtx.Streaming
                         where srm.Type == StreamingType.Filter &&
                                srm.Track == "sdpf"
                         select srm;

            cancellationToken.Register(
                () => Environment.Exit(-1));

            #region Log

            var logId = Constants.Singleton;
            var logProxy = ActorProxy.Create<IImdbFaults>(logId);
            await logProxy.Report($"Twitter Start Listening");

            #endregion // Log

            await stream.StartAsync(async context =>
            {
                if (context.Entity == null && context.EntityType != StreamEntityType.Status)
                    return;
                var status = (Status)context.Entity;
                var url = status.Entities.UrlEntities.FirstOrDefault()?.ExpandedUrl;

                var input = new Input
                {
                    UserImageUrl = status.User.ProfileImageUrl,
                    UserName = status.User.Name
                };

                var id = new ActorId(url); // cache process effort by routing to the same actor
                var proxy = ActorProxy.Create<IImdb>(id, "fabric:/IMDB_Fabric");
                if (!await proxy.TryProcess(input))
                    Trace.WriteLine($"Fault url [{url}]");
                else
                    await logProxy.Report($"Twitt {url}");
            });

            await Task.Delay(TimeSpan.FromSeconds(0.5), cancellationToken);
        }

        #endregion // RunAsync 

        #region ClearOldTwits

        private static async Task ClearOldTwits(TwitterContext twitterCtx)
        {
            var statuses = (from status in twitterCtx.Status
                               where status.Type == StatusType.User &&
                                    status.Entities.HashTagEntities.Any(h=>h.Tag == "sdpf")
                               select status.StatusID).ToArray();
            var tasks = from id in statuses
                      select twitterCtx.DeleteTweetAsync(id);
            await Task.WhenAll(tasks);
        }

        #endregion // ClearOldTwits

        #region AuthorizeAsync

        /// <summary>
        /// Authorizes against Twitter.
        /// </summary>
        /// <returns></returns>
        private async Task<IAuthorizer> AuthorizeAsync()
        {
            ServiceEventSource.Current.ServiceMessage(this, "Twitter: Authorizing");
            try
            {
                string tmp = CloudConfigurationManager.GetSetting("IsLocal");
                bool isLocal;
                string consumerKey, consumerSecret, accessToken, accessTokenSecret;
                //if (bool.TryParse(tmp, out isLocal) && isLocal)
                //{
                //    consumerKey = Environment.GetEnvironmentVariable("TweeterConsumerKey");
                //    consumerSecret = Environment.GetEnvironmentVariable("TweeterConsumerSecret");
                //    accessToken = Environment.GetEnvironmentVariable("TwitterAccessToken");
                //    accessTokenSecret = Environment.GetEnvironmentVariable("TwitterAccessTokenSecret");
                //}
                //else
                //{
                //    consumerKey = CloudConfigurationManager.GetSetting("TwittererConsumerKey");
                //    consumerSecret = CloudConfigurationManager.GetSetting("TwittererConsumerSecret");
                //    accessToken = CloudConfigurationManager.GetSetting("TwitterAccessToken");
                //    accessTokenSecret = CloudConfigurationManager.GetSetting("TwitterAccessTokenSecret");
                //}
                    consumerKey = "H8p54995Li7jPmVLJ8bB9mQB0";
                    consumerSecret = "weCBWgqb59b7788rQPybz3c0v8Jaib3478DtnRIN37NgDago0N";
                    accessToken = "3411603499-c99yJa5wWoVUYXcypK7nKM8MyUWKXmnVGO9maS9";
                    accessTokenSecret = "XLKGR6VsXpTsjLx4E5ZaL20cCp8Yoc42mlMe3ZW6tx7hC";

                var auth = new SingleUserAuthorizer()
                {
                    CredentialStore = new SingleUserInMemoryCredentialStore
                    {
                        ConsumerKey = consumerKey,
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
                var id = Constants.Singleton;
                var proxy = ActorProxy.Create<IImdbFaults>(id);
                await proxy.ReportError($"Twitter Authorization failed: {ex}");

                ServiceEventSource.Current.ServiceHostInitializationFailed(ex);
                throw;
            }

            #endregion // Exception Handling
        }

        #endregion // AuthorizeAsync
    }
}
