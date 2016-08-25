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
                return View(new WallViewModel {InviteCode = wall.InviteCode});
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