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
    public class Imdb : Actor<ImdbItemRawState>, IImdb
    {
        private const int DOWNLOAD_TIMEOUT_SEC = 10;

        public override async Task OnActivateAsync()
        {
            if (State.Name == null)
            {
                ActorEventSource.Current.ActorMessage(
                    this, $"Loading Data for: {Id.GetStringId()}");
                await LoadStateAsync();
            }
        }

        public async Task Process(Input data)
        {
            var id = new ActorId("PUBLISH"); // should be singleton so the UI can listen to specific actor (like a topic)
            var actor = ActorProxy.Create<IOutputPublisher>(id);
            switch (State.Type)
            {
                case ImdbType.Movie:
                    var m = new Movie(State.Name, State.Date.Year, State.ImageUrl);
                    await actor.SendMovieAsync(m);
                    break;
                case ImdbType.Actor:
                    var star = new Star(State.Name, State.Date, State.ImageUrl);
                    await actor.SendStarAsync(star);
                    break;
                case ImdbType.Unknown:
                default:
                    throw new NotSupportedException("Invalid type");
            }
        }

        #region LoadStateAsync

        /// <summary>
        /// Loads the state.
        /// </summary>
        /// <returns></returns>
        private async Task LoadStateAsync()
        {
            State = new ImdbItemRawState { Url = Id.GetStringId() };
            string html = await DownloadAsync();
            ParseAsync(html);
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
            doc.LoadHtml(html);

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
                State.Type = ImdbType.Actor;
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
            else if (State.Type == ImdbType.Actor)
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
