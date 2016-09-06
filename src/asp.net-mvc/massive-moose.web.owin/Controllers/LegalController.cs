using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace massive_moose.web.owin.Controllers
{
    public class LegalController : Controller
    {
        // GET: Legal
        public ActionResult PrivacyPolicy()
        {
            return View();
        }

        public ActionResult TermsOfUse()
        {
            return View();
        }
    }
}