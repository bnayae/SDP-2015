namespace IMDB.Services.Controllers
{
    using Interfaces;
    using Microsoft.ServiceFabric.Actors;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;

    [RoutePrefix("api")]
    public class DefaultController : ApiController
    {
        // GET api/top-movies
        [Route("logs")]
        [HttpGet]
        public Task<LogItem[]> GetLogs()
        {
            var id = Constants.Singleton;
            var proxy = ActorProxy.Create<IImdbFaults>(id);
            return proxy.GetAsync();
        }

        // GET api/top-movies
        [Route("top-movies")]
        [HttpGet]
        public Task<ProfileRate[]> GetMovies()
        {
            var id = new ActorId(ImdbType.Movie.ToString());
            var proxy = ActorProxy.Create<IImdbTopRated>(id);
            return proxy.Get();
        }

        [Route("top-stars")]
        [HttpGet]
        public Task<ProfileRate[]> GetStars()
        {
            var id = new ActorId(ImdbType.Star.ToString());
            var proxy = ActorProxy.Create<IImdbTopRated>(id);
            return proxy.Get();
        }       
    }
}
