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
        public ActionResult Create(int id = StoryType.DefaultStoryType)
        {
            var model = TagSystem.CreateStoryResponseModel(id);
            return View(model);
        }
    }
}
