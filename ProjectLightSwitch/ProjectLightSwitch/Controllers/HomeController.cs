using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjectLightSwitch.Models;

namespace ProjectLightSwitch.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
            return View();
        }

        public ActionResult Eric()
        {
                using (var context = new StoryModel())
                {
                    var foo = new StoryType();
                    context.StoryTypes.Add(foo);
                    context.SaveChanges();
                    return RedirectToAction("Index");
                }

            return View();
            //return Json(TagSystem.GetFullTagNavigationPath_Json(26, null), JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}
