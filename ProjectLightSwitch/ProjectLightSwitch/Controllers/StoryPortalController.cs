using System;
using ProjectLightSwitch.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectLightSwitch.Controllers
{
    public class StoryPortalController : Controller
    {
        //
        // GET: /StoryPortal/

        public ActionResult Index()
        {
            using (var context = new StoryModel())
            {
                StoryPortalViewModel model = new StoryPortalViewModel();

                model.TopRatedStoryIds = context.StoryResponses.OrderByDescending(t => t.Ratings.Count).Take(3)
                                                               .Select(id => id.StoryResponseId);


                return View(model);
            }
        }
        public ActionResult Create(int story)
        {
            var model = TagSystem.CreateStoryResponseModel(story);
            return View(model);
        }
    }
}
