using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using log4net;
using massive_moose.services.models;
using massive_moose.services;

namespace massive_moose.api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class V2Controller : ApiController
    {
        private static ILog Log = LogManager.GetLogger(typeof(V2Controller));
        [HttpGet]
        [Route("v2/wall/{originX}/{originY}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public Brick[,] Wall(int originX, int originY)
        {
            using (var session = SessionFactory.Instance.OpenSession())
            {
                var bricks = session.CreateCriteria<Brick>()
                    .List<Brick>();

                var wall = new Brick[12, 12];
                for (int y = 0; y < 12; y++)
                {
                    for (int x = 0; x < 12; x++)
                    {
                        var relativeX = originX - 6 + x;
                        var relativeY = originY - 6 + y;
                        wall[x, y] = bricks.SingleOrDefault(b => b.AddressX == relativeX && b.AddressY == relativeY);
                        if (wall[x, y] == null)
                            wall[x, y] = new Brick() {AddressX=relativeX, AddressY=relativeY,Guid=Guid.Empty,Id=0};
                    }
                }

                return wall;
            }
        }
    }
}
