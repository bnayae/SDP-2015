using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace IMDB_Hub.Controllers
{
    [Route("api/[controller]")]
    public class PublishController : Controller
    {
        // GET: api/publish
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/publish/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/publish
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/publish/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/publish/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
