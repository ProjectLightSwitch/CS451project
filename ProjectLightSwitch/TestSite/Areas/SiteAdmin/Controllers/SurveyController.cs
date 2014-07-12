using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestSite.Models;



namespace TestSite.Areas.SiteAdmin.Controllers
{
    public class SurveyController : Controller
    {
        //
        // GET: /SiteAdmin/Survey/
        // TODO: this might be better suited for a separate tag controller
        public ActionResult Index()
        {
            var ctx = new TagSystem();
            var model = ctx.GetTagsByType(showCategories: true);
            return View(model);
        }

        // TODO: this might be better suited for a separate tag controller
        public ActionResult View(string search, int id = 0)
        {
            var ctx = new TagSystem();
            Tag tag = ctx.GetTag(id);
            int catId = (tag == null) ? 0 : tag.TagId;
            var paths = ctx.GetPaths(catId, search, true);
            ViewBag.Category = tag;
            return View(paths);
        }

        public ActionResult AddChildTag(int category, int id = 0)
        {
            var ctx = new TagSystem();
            // Hard coded for now
            ctx.AddTag(new Tag { EnglishText = Guid.NewGuid().ToString(), TagType = (byte)TagSystem.TagType.Tag }, id);

            ViewBag.Message = "Tag Added";
            return RedirectToAction("View", new { id = category });
        }



        public ActionResult RemoveTag(int category, int id)
        {
            var ctx = new TagSystem();
            bool result = ctx.RemoveTag(id);
            ViewBag.Message = result ? "Tag was deleted" : "Tag could not be deleted";
            return RedirectToAction("View", new { id = category });
        }
    }
}
