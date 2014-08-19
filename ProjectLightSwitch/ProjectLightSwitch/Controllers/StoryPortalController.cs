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

        public ActionResult Home()
        {
            using (var context = new StoryModel())
            {
                StoryPortalViewModel model = new StoryPortalViewModel();

                model.TopRatedStoryIds = context.StoryResponses.OrderByDescending(t => t.StoryResponseRatings.Count).Take(3)
                                                               .Select(id => id.StoryResponseId);


                return View(model);
            }
        }
        public ActionResult Create(int id = StoryType.DefaultStoryTypeId)
        {
            var model = TagSystem.CreateStoryResponseModel(id);
            return View(model);
        }
    }
}
