using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IMDB.Interfaces;
using Microsoft.ServiceFabric;
using Microsoft.ServiceFabric.Actors;
using System.Net.Http;
using System.Xml.Linq;
using HtmlAgilityPack;
using System.IO;
using System.Diagnostics;

namespace IMDB
{
    /// <summary>
    /// IMDB processing 
    /// meant to be instantiate with instance per URL (Actor Id)
    /// </summary>
    [VolatileActorStateProvider] // replicate in memory state
    public class Imdb : StatefulActor<ImdbItemRawState>, IImdb
    {
        private const int DOWNLOAD_TIMEOUT_SEC = 10;
        private IImdbHub _hub;

        protected override async Task OnActivateAsync()
        {
            if (State == null)
                await LoadStateAsync(); // load once per instance rather per call

             var id = Constants.Singleton; // should be singleton so the UI can listen to specific actor (like a topic)
            _hub = ActorProxy.Create<IImdbHub>(id);
        }

        #region TryProcess

        public async Task<bool> TryProcess(Input data)
        {
            var sender = new Profile(data.UserName, data.UserImageUrl);

            Profile item = new Profile(State.Name, State.ImageUrl);
            var twittData = new TwittData (item, sender);
            switch (State.Type)
            {
                case ImdbType.Movie:
                    await _hub.SendMovieAsync(twittData);
                    break;
                case ImdbType.Star:
                    await _hub.SendStarAsync(twittData);
                    break;
            }

            var counterId = new ActorId(State.Name);
            var counterProxy = ActorProxy.Create<IImdbCounter>(counterId);
            await counterProxy.IncrementAsync(State.Type, item);
            return true;
        }

        #endregion // TryProcess

        #region LoadStateAsync

        /// <summary>
        /// Loads the state.
        /// </summary>
        /// <returns></returns>
        private async Task LoadStateAsync()
        {
            #region Log

            ActorEventSource.Current.ActorMessage(
                this, $"Loading Data for: {Id.GetStringId()}");

            #endregion // Log

            try
            {
                State = new ImdbItemRawState { Url = Id.GetStringId() };
                string html = await DownloadAsync();
                ParseStateAsync(html);
            }
            #region Exception Handling

            catch (Exception ex)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(ex);
                var id = Constants.Singleton;
                var proxy = ActorProxy.Create<IImdbFaults>(id);
                await proxy.ReportParsingError(State.Url);
            }

            #endregion // Exception Handling
        }

        #endregion // LoadStateAsync

        #region DownloadAsync

        /// <summary>
        /// Downloads the html content.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">${this.GetType().Name} state loading failure: {response.StatusCode}, {response.ReasonPhrase}</exception>
        private async Task<string> DownloadAsync()
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(DOWNLOAD_TIMEOUT_SEC);

            HttpResponseMessage response = await client.GetAsync(State.Url);
            if (!response.IsSuccessStatusCode) // TODO: publish the error the Fault handler actor
                throw new Exception($"{this.GetType().Name} state loading failure: {response.StatusCode}, {response.ReasonPhrase}");

            HttpContent content = response.Content;
            string html = await content.ReadAsStringAsync();
            return html;
        }

        #endregion // DownloadAsync

        #region ParseStateAsync

        /// <summary>
        /// Parses the asynchronous.
        /// </summary>
        /// <param name="html">The HTML.</param>
        private void ParseStateAsync(string html)
        {
            #region XElement e = ...

            HtmlDocument doc = new HtmlDocument();
            doc.OptionOutputAsXml = true;
            doc.LoadHtml(html.Trim());

            XElement e;
            using (var srm = new MemoryStream())
            {
                doc.Save(srm);
                srm.Seek(0, SeekOrigin.Begin);
                e = XElement.Load(srm);
            }

            #endregion // XElement e = ...

            #region State.Type = ... (movie or star)

            string type = e.Descendants()
                           .FirstOrDefault(
                                x => x.Name == "meta" &&
                                        x.Attribute("property")?.Value == "og:type")
                            ?.Attribute("content")
                            ?.Value;
            if (string.IsNullOrEmpty(type))
                throw new NullReferenceException("IMDB's type");
            if (type == "video.movie")
                State.Type = ImdbType.Movie;
            else if (type == "actor")
                State.Type = ImdbType.Star;
            else
                throw new Exception("Not supported format");

            #endregion // State.Type = ... (movie or star)

            #region State.Name = ...

            State.Name = e.Descendants()
                           .FirstOrDefault(
                                x => x.Name == "span" &&
                                        x.Attribute("itemprop")?.Value == "name")
                            ?.Value;

            if (string.IsNullOrEmpty(State.Name))
                throw new NullReferenceException("movie's name");

            #endregion // State.Name = ...

            #region State.ImageUrl = ...

            State.ImageUrl = e.Descendants()
                           .FirstOrDefault(
                                x => x.Name == "img" &&
                                        x.Attribute("itemprop")?.Value == "image")
                            ?.Attribute("src")
                            ?.Value;
            if (string.IsNullOrEmpty(State.ImageUrl))
                throw new NullReferenceException("movie's image");

            #endregion // State.ImageUrl = ...
        }

        #endregion // ParseStateAsync
    }
}
