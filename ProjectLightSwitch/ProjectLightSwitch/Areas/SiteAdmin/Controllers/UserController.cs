using ProjectLightSwitch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ProjectLightSwitch.Areas.SiteAdmin.Controllers
{
    [Authorize(Roles=SiteSettings.AdminRole)]
    public class UserController : Controller
    {

        public const string AdminRole = "Administrator";

        //
        // GET: /SiteAdmin/User/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddAdmin(string username)
        {
            if(!Roles.RoleExists(SiteSettings.AdminRole))
            {
                Roles.CreateRole(SiteSettings.AdminRole);
            }
            Roles.AddUserToRole(username, SiteSettings.AdminRole);

            HelperFunctions.AddGlobalMessage(TempData, "Admin added");

            return RedirectToAction("Index");
        }
    }
}
