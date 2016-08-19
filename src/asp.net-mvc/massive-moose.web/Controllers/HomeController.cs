using massive_moose.contracts;
using massive_moose.data;
using massive_moose.web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace massive_moose.web.Controllers
{
    public class HomeController : Controller
    {
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
                        wallVm.Bricks[i,j]=new BrickViewModel()
                        {
                            AddressX = j,
                            AddressY = i,
                            ImageUrl = (b != null
                                ? string.Format("{0}/v1/image/{1}/{2}",
                                    "http://local.api.massivemoose.com", j, i)
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

        [Route("sl")]
        public ActionResult Silverlight()
        {
            return View();
        }
    }
}