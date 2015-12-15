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
        private Task<Profile> _info;
        private IImdbHub _hub;

        protected override async Task OnActivateAsync()
        {
            if (State == null)
            {
                ActorEventSource.Current.ActorMessage(
                    this, $"Loading Data for: {Id.GetStringId()}");
                await LoadStateAsync();
                var info = new Profile(State.Name, State.ImageUrl);
                _info = Task.FromResult(info);
            }

             var id = Constants.Singleton; // should be singleton so the UI can listen to specific actor (like a topic)
            _hub = ActorProxy.Create<IImdbHub>(id);
        }

        public Task<Profile> GetInfo() => _info;

        #region TryProcess

        public async Task<bool> TryProcess(Input data)
        {
            var sender = new Profile(data.UserName, data.UserImageUrl);

            Profile item;
            switch (State.Type)
            {
                case ImdbType.Movie:
                    var movie = new Movie(State.Name, State.Date.Year, State.ImageUrl, sender);
                    await _hub.SendMovieAsync(movie);
                    item = new Profile(movie.Name, movie.ImageUrl);
                    break;
                case ImdbType.Star:
                    var star = new Star(State.Name, State.Date, State.ImageUrl, sender);
                    await _hub.SendStarAsync(star);
                    item = new Profile(star.Name, star.ImageUrl);
                    break;
                case ImdbType.Unknown:
                default:
                    return false; // initialization error
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
            try
            {
                State = new ImdbItemRawState { Url = Id.GetStringId() };
                string html = await DownloadAsync();
                ParseAsync(html);
            }
            catch (Exception ex)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(ex);
                var id = Constants.Singleton;
                var proxy = ActorProxy.Create<IImdbFaults>(id);
                await proxy.ReportParsingError(State.Url);
            }
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

        #region ParseAsync

        /// <summary>
        /// Parses the asynchronous.
        /// </summary>
        /// <param name="html">The HTML.</param>
        private void ParseAsync(string html)
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

            State.Name = e.Descendants()
                           .FirstOrDefault(
                                x => x.Name == "span" &&
                                        x.Attribute("itemprop")?.Value == "name")
                            ?.Value;

            if (string.IsNullOrEmpty(State.Name))
                throw new NullReferenceException("movie's name");

            State.ImageUrl = e.Descendants()
                           .FirstOrDefault(
                                x => x.Name == "img" &&
                                        x.Attribute("itemprop")?.Value == "image")
                            ?.Attribute("src")
                            ?.Value;
            if (string.IsNullOrEmpty(State.ImageUrl))
                throw new NullReferenceException("movie's image");

            if (State.Type == ImdbType.Movie)
            {
                string dateText = e.Descendants()
                              .FirstOrDefault(
                                   x => x.Name == "meta" &&
                                           x.Attribute("itemprop")?.Value == "datePublished")
                               ?.Attribute("content")
                               ?.Value;
                DateTime datePublished;
                if (!DateTime.TryParse(dateText, out datePublished))
                    throw new NullReferenceException("movie's date");
                State.Date = datePublished;
            }
            else if (State.Type == ImdbType.Star)
            {
                string dateText = e.Descendants()
                              .FirstOrDefault(
                                   x => x.Name == "time" &&
                                           x.Attribute("itemprop")?.Value == "birthDate")
                               ?.Attribute("datetime")
                               ?.Value;
                DateTime birthDate;
                if (!DateTime.TryParse(dateText, out birthDate))
                    throw new NullReferenceException("movie's date");
                State.Date = birthDate;
            }
        }

        #endregion // ParseAsync
    }
}
