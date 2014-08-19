using ProjectLightSwitch.Attributes;
using ProjectLightSwitch.Models;
using ProjectLightSwitch.Models.Enums;
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
        // GET: /Ancestors/
        public ActionResult Index()
        {
            return View();
        }

        // TODO: make partial
        public ActionResult TagNavigator()
        {
            return PartialView("~/Views/Shared/_TagNavigator.cshtml", TagSystem.GetCategories());
        }

        //[AjaxOnly]
        [OutputCache(Duration = 30, VaryByParam = "*", VaryByHeader = "Accept-Language")]
        public ActionResult Navigate(int id, bool childrenOnly = false, int language = Language.DefaultLanguageId)
        {
            return Content(TagSystem.GetFullTagNavigationPath_Json(id, childrenOnly, language), "application/json");
        }

        [AjaxOnly]
        [HttpGet]
        public ActionResult Search(string term)
        {
            return Content(TagSystem.GetPaths_Json(TagTree.InvisibleRootId, term, true, SiteSettings.DefaultLanguageId), "application/json");
        }

        //[AjaxOnly]
        //public ActionResult AddChildTag(int tagId, string text, byte type, int languageId = Language.DefaultLanguageId)
        //{
        //    if(!Enum.IsDefined(typeof(TagType), type) || type == TagTree.InvisibleRootId)
        //    {
        //        return Content("{'error':'Invalid Type.'}", "application/json");
        //    }

        //    using (var context = new StoryModel())
        //    {
        //        var tagType = (TagType)type;

        //        TagSystem.AddTag(new Ancestor() { TagType = type }, tagId);
        //        TagSystem.AddTag(type, tagId, 

        //        var item = context.Tags.Where(t => t.TagId == type && t.TagId != TagTree.InvisibleRootId).FirstOrDefault();
        //        if (item != null)
        //        {
        //            context.Tags.Remove(item);
        //            bool success = context.SaveChanges() > 0;
        //            return Json(new { result = success, error = success ? null : "There was an error removing the tag." });
        //        }
        //        return Content("{'error':'Ancestors not found.'}", "application/json");
        //    }
        //}

        [AjaxOnly]
        public ActionResult RemoveTag(int tagId)
        {
            using (var context = new StoryModel())
            {
                var item = context.Tags.Where(t => t.TagId == tagId && t.TagId != TagTree.InvisibleRootId).FirstOrDefault();
                if (item != null)
                {
                    context.Tags.Remove(item);
                    bool success = context.SaveChanges() > 0;
                    return Json(new { error = success ? null : "There was an error removing the tag." });
                }
                return Content("{'error':'Ancestors not found.'}", "application/json");
            }
        }
    }
}
