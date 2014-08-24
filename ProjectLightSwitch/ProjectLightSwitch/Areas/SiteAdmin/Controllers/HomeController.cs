using ProjectLightSwitch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectLightSwitch.Areas.SiteAdmin.Controllers
{
    [Authorize(Roles = SiteSettings.AdminRole)]
    public class HomeController : Controller
    {
        //
        // GET: /SiteAdmin/Site/

        public ActionResult Index()
        {
            return View();
        }

    }
}
