using massive_moose.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using massive_moose.web.owin.Models;
using log4net;
using massive_moose.services.models;
using System.Configuration;
using NHibernate;
using NHibernate.Criterion;

namespace massive_moose.web.owin.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILog _log;
        private readonly WallOperations _wallOperations;

        public HomeController(ILog log, WallOperations wallOperations)
        {
            _log = log;
            _wallOperations = wallOperations;
        }

        [Route("{inviteCode}")]
        public ActionResult Index(string inviteCode=null)
        {
            using (var session = SessionFactory.Instance.OpenStatelessSession())
            {
                var wall = _wallOperations.GetWallByKeyOrDefault(inviteCode, session);
                return View(new WallViewModel {InviteCode = wall.InviteCode, Bricks = GetBricksForWall(0,0,inviteCode,session)});
            }
        }

        private BrickViewModel[,] GetBricksForWall(int originX, int originY, string wallKey, IStatelessSession session)
        {
            Wall wallRecord = _wallOperations.GetWallByKeyOrDefault(wallKey, session);

            IList<object[]> bricks = session.QueryOver<Brick>()
                .And(b => b.Wall.Id == wallRecord.Id)
                .Select(
                    b => b.AddressX,
                    b => b.AddressY,
                    b => b.LastUpdated,
                    b => b.Guid).List<object[]>();

            var wall = new BrickViewModel[12, 12];
            for (int y = 0; y < 12; y++)
            {
                for (int x = 0; x < 12; x++)
                {
                    var relativeX = originX - 6 + x;
                    var relativeY = originY - 6 + y;
                    var o = bricks.SingleOrDefault(b => (int)b[0] == relativeX && (int)b[1] == relativeY);
                    if (o != null)
                    {
                        wall[x, y] = new BrickViewModel()
                        {
                            AddressX = (int)o[0],
                            AddressY = (int)o[1],
                            DateUpdated = ((DateTime?)o[2]).HasValue ? ((DateTime?)o[2]).Value.Ticks.ToString() : "",
                            HasContent=true,
                            ThumbnailImageUrl = _wallOperations.GetThumbnailImageUrl(wallKey, (int)o[0],(int)o[1])
                        };
                    }
                    else
                    {
                        wall[x, y] = new BrickViewModel()
                        {
                            AddressX = relativeX,
                            AddressY = relativeY
                        };
                    }
                }
            }

            return wall;
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public ActionResult CreateDatabase()
        {
                SessionFactory.RebuildDatabase();
                return new ContentResult()
                {
                    Content = "Done"
                };
        }
    }
}