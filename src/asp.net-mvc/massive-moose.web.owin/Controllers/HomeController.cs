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
using massive_moose.services.viewmodels;
using NHibernate;
using NHibernate.Criterion;
using Newtonsoft.Json;

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
            MassiveMooseCookie cookie = null;
            if (Request.Cookies.AllKeys.Contains("massivemoose"))
            {
                cookie = JsonConvert.DeserializeObject<MassiveMooseCookie>(Request.Cookies.Get("massivemoose").Value);
            }

            using (var session = SessionFactory.Instance.OpenStatelessSession())
            {
                var wall = _wallOperations.GetWallByKeyOrDefault(inviteCode, session);
                return View(new WallViewModel
                {
                    InviteCode = wall.InviteCode,
                    Bricks = _wallOperations.GetWallSnapshot(0,0,inviteCode,session),
                    BackgroundImageUrl=string.Format("{0}/v1/background/{1}",
                    ConfigurationManager.AppSettings["MMApi"],
                    string.IsNullOrWhiteSpace(wall.BackgroundImageFilename) ? "white-brick.jpg" : wall.BackgroundImageFilename),
                    DontHelpMe=(cookie != null ? cookie.DontHelpMe : false)
                });
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

        [HttpGet]
        public ActionResult ColorPicker()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Help()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DontHelpMe()
        {
            MassiveMooseCookie cookie = null;
            if (Request.Cookies.AllKeys.Contains("massivemoose"))
            {
                cookie = JsonConvert.DeserializeObject<MassiveMooseCookie>(Request.Cookies.Get("massivemoose").Value);
            }
            else
            {
                cookie= new MassiveMooseCookie();
            }
            cookie.DontHelpMe = true;
            Response.Cookies.Set(new HttpCookie("massivemoose", JsonConvert.SerializeObject(cookie)));
            return new EmptyResult();
        }
        
        [HttpGet]
        public ActionResult Log()
        {
            var logFilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/app_data/log.txt");
            if (System.IO.File.Exists(logFilePath))
            {
                using (var file = System.IO.File.Open(logFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.Read))
                using (System.IO.StreamReader reader = new StreamReader(file))
                {
                    return Content(new StringContent(reader.ReadToEnd(), Encoding.UTF8, "text/plain"));
                }
            }
            return HttpNotFound();
}
    }
}
