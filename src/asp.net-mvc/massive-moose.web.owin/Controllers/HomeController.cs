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

namespace massive_moose.web.owin.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILog _log;

        public HomeController(ILog log)
        {
            _log = log;
        }

        public ActionResult Index()
        {
            using (var session = SessionFactory.Instance.OpenStatelessSession())
            {
                var bricks = session.CreateCriteria<Brick>()
                    .List<Brick>();

                var wallVm = new WallViewModel();

                for (int i = 0; i < 12; i++)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        var b = bricks.SingleOrDefault(br => br.AddressX == j && br.AddressY == i);
                        wallVm.Bricks[i, j] = new BrickViewModel()
                        {
                            AddressX = j,
                            AddressY = i,
                            ImageUrl = (b != null
                                ? string.Format("{0}/v1/image/{1}/{2}",
                                    ConfigurationManager.AppSettings["MMApi"], j, i)
                                : null)
                        };
                    }
                }
                return View(wallVm);
            }
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