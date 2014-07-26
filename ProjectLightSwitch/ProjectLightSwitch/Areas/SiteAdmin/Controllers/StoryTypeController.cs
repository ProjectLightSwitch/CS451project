using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjectLightSwitch.Models;


namespace ProjectLightSwitch.Areas.SiteAdmin.Controllers
{
    public class StoryTypeController : Controller
    {
        //
        // GET: /SiteAdmin/Survey/
        // TODO: this might be better suited for a separate tag controller
        public ActionResult Index()
        {
            var model = TagSystem.GetTagsByType(showCategories: true);
            return View(model);
        }
        
        public ActionResult Created(object xxx)
        {
            return null;
        }

        public ActionResult Create()
        { 




            return View();
        }

        // TODO: this might be better suited for a separate tag controller
        public ActionResult View(string search, int id = 0)
        {
            Tag tag = TagSystem.GetTag(id);
            int catId = (tag == null) ? 0 : tag.TagId;
            var paths = TagSystem.GetPaths(catId, search, true);
            ViewBag.Category = tag;
            return View(paths);
        }

        public ActionResult AddChildTag(int category, int id = 0)
        {
            // Hard coded for now
            TagSystem.AddTag(new Tag { EnglishText = Guid.NewGuid().ToString(), TagType = (byte)ProjectLightSwitch.Models.Enums.TagType.Tag }, id);

            ViewBag.Message = "Tag Added";
            return RedirectToAction("View", new { id = category });
        }



        public ActionResult RemoveTag(int category, int id)
        {
            bool result = TagSystem.RemoveTag(id);
            ViewBag.Message = result ? "Tag was deleted" : "Tag could not be deleted";
            return RedirectToAction("View", new { id = category });
        }
    }
}
