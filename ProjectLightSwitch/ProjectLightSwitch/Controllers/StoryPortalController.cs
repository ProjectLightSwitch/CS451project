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
            return View();
        }
        public ActionResult Create(int story)
        {
            var model = TagSystem.CreateStoryResponseModel(story);
            return View(model);
        }
    }
}
