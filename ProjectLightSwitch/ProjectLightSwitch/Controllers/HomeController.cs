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
            return View();
            //return Json(TagSystem.GetFullTagNavigationPath_Json(26, null), JsonRequestBehavior.AllowGet);
        }

        public ActionResult TagSearchTest()
        {
            return View();
        }

        [HttpPost]
        public ActionResult TagSelTest(List<int> SelectedTags)
        {
            if (ModelState.IsValid)
            {
                return Content("Selected Tags: " + String.Join(", ", SelectedTags), "text/text");
            }
            else
            {
                return Content("ERROR", "text/text");
            }
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

        public ActionResult PageHome()
        {
            return View();
        }

        public ActionResult PageP1()
        {
            return View();
        }

        public ActionResult PageP2()
        {
            return View();
        }

        public ActionResult PageP3()
        {
            return View();
        }
    }
}
