namespace IMDB.Services
{
    // read more at http://www.asp.net/signalr/overview/guide-to-the-api/hubs-api-guide-server
    using Interfaces;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;
    using System;
    using System.Collections.Generic;
    using System.Web.Http;

    [HubName(Constants.HubName)]
    public class ImdbHub : Hub
    {
        public void LikeMovie(Movie movie)
        {
            try
            {
                Clients.All.BroadcastLikeMovie(movie);
            }
            #region Exception Handling

            catch (Exception ex)
            {
                ServiceEventSource.Current.ErrorMessage(ex);
            }

            #endregion // Exception Handling
        }

        public void LikeStar(Star star)
        {
            try
            {
                Clients.All.BroadcastLikeStar(star);
            }
            #region Exception Handling

            catch (Exception ex)
            {

                ServiceEventSource.Current.ErrorMessage(ex);
            }

            #endregion // Exception Handling
        }

        public void Changed(ImdbType type, ProfileRate[] items)
        {
            try
            {
                Clients.All.BroadcastChanged(type, items);
            }
            #region Exception Handling

            catch (Exception ex)
            {

                ServiceEventSource.Current.ErrorMessage(ex);
            }

            #endregion // Exception Handling
        }

        public void ParserError(string url)
        {
            try
            {
                Clients.All.BroadcastParserError(url);
            }
            #region Exception Handling

            catch (Exception ex)
            {

                ServiceEventSource.Current.ErrorMessage(ex);
            }

            #endregion // Exception Handling
        }
    }
}
