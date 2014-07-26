using ProjectLightSwitch.Attributes;
using ProjectLightSwitch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectLightSwitch.Controllers
{
    public class TagsController : Controller
    {
        //
        // GET: /Tag/
        public ActionResult Index()
        {
            return View();
        }

        // TODO: make partial
        public ActionResult TagNavigator()
        {
            return PartialView("~/Views/Shared/_TagNavigator.cshtml", TagSystem.GetCategories());
        }

        public ActionResult GetCategories()
        {
            return Json(TagSystem.GetCategories(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetChildTags(int parent)
        {
            return Json(TagSystem.GetChildTags(parent, false), JsonRequestBehavior.AllowGet);
        }

    }
}
