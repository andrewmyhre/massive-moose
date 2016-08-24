using massive_moose.services.models;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace massive_moose.api.Controllers
{
    [Authorize]
    public class BricksController : ApiController
    {
        // GET api/values
        public IEnumerable<Brick> Get(int minX, int minY, int maxX, int maxY)
        {
            return new Brick[]
            {
                new Brick() {AddressX=0,AddressY=0,Guid=Guid.NewGuid(),Id=1}, 
            };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
