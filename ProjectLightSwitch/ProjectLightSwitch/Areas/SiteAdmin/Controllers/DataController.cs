using ProjectLightSwitch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectLightSwitch.Areas.SiteAdmin.Controllers
{
    [Authorize(Roles = SiteSettings.AdminRole)]
    public class DataController : Controller
    {
        //
        // GET: /SiteAdmin/Data/

        public ActionResult Index()
        {
            return Content("home");
        }

        public ActionResult ResetData()
        {
            TagSystem.ResetData();
            return Content("Done");
        }

        public ActionResult SeedData()
        {
            TagSystem.SeedData();
            return Content("Done");
        }
    }
}
