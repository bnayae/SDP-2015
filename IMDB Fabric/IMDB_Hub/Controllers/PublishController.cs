using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using IMDB.Interfaces;

namespace IMDB_Hub.Controllers
{
    [Route("api/[controller]")]
    public class PublishController : Controller
    {
        // GET: api/publish
        [HttpGet]
        public string Get()
        {
            return "OK";
        }
     
        // POST api/publish/movie
        [HttpPost("movie")]
        public void PostMovie([FromBody]Movie value)
        {
        }

        // POST api/publish/movie
        [HttpPost("star")]
        public void PostStar([FromBody]Star value)
        {
        }
    }
}
