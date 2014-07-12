using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestSite.Models;

namespace TestSite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //var ctx = new TagSystem();
            //int id = ctx.AddTag("The first category", "the first category in spanish", 0, TagSystem.TagType.Category);
            //ctx.AddTag("The second cat", "the second cat in spanish", 0, TagSystem.TagType.Category);
            //int id2 = ctx.AddTag("The first top-level category", "The first top-level category in spanish", id, TagSystem.TagType.TopLevelTag);

            //ViewBag.Message = "Created path is " + String.Join(" > ", ctx.GetPathById(id2));

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult SeedData()
        {
            var ctx = new TagSystem();
            ctx.SeedData();

            return Content("Done");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult TagPathTest(string q = "tag_1")
        {
            // List<IGrouping<int, TagSystem.TagSummary>>
            var model = new TagSystem().GetPaths(-1, q, true);
            ViewData["SearchTerm"] = q;
            return View(model);
        }
    }
}
