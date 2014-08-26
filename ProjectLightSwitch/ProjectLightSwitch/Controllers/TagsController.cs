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
        // GET: /Descendants/
        public ActionResult Index()
        {
            return View();
        }

        // TODO: make partial
        //public ActionResult TagNavigator()
        //{
        //    return PartialView("~/Views/Shared/_TagNavigator.cshtml", TagSystem.GetCategories());
        //}

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
            return Content(TagSystem.GetPaths_Json(TagTree.InvisibleRootId, term, true, Language.DefaultLanguageId), "application/json");
        }

        //[AjaxOnly]
        //public ActionResult AddChildTag(int tagId, string text, byte storyType, int languageId = Language.DefaultLanguageId)
        //{
        //    if(!Enum.IsDefined(typeof(TagType), storyType) || storyType == TagTree.InvisibleRootId)
        //    {
        //        return Content("{'error':'Invalid Type.'}", "application/json");
        //    }

        //    using (var context = new StoryModel())
        //    {
        //        var tagType = (TagType)storyType;

        //        TagSystem.AddTag(new Ancestor() { TagType = storyType }, tagId);
        //        TagSystem.AddTag(storyType, tagId, 

        //        var item = context.Tags.Where(t => t.TagId == storyType && t.TagId != TagTree.InvisibleRootId).FirstOrDefault();
        //        if (item != null)
        //        {
        //            context.Tags.Remove(item);
        //            bool success = context.SaveChanges() > 0;
        //            return Json(new { result = success, error = success ? null : "There was an error removing the tag." });
        //        }
        //        return Content("{'error':'Descendants not found.'}", "application/json");
        //    }
        //}

    //    public ActionResult RemoveTag(int TagId)
    //    {
    //        var parent = TagSystem.GetParent(TagId);
    //        string message = TagSystem.RemoveTag(TagId) ? "Ancestor and all child tags removed." : "There was an error removing the tag";
    //        HelperFunctions.AddGlobalMessage(TempData, message);
    //        return RedirectToAction("Index", (parent != null) ? new { id = parent.TagId } : null);
    //    }

    //    public ActionResult AddChildTags(List<TagViewModel> model)
    //    {
    //        var childrenToAdd = new List<TagViewModel>(model);

    //        model.RemoveAll(m => string.IsNullOrWhiteSpace(m.EnglishText));

    //        if (model.Count > 0 && ModelState.IsValid)
    //        {
    //            foreach(var child in model)
    //            {
    //                //don't add tags with no english tag
    //                if (String.IsNullOrWhiteSpace(child.Translations[Language.DefaultLanguageId]))
    //                { 
    //                    childrenToAdd.Remove(child);
    //                    HelperFunctions.AddGlobalMessage(TempData, "A tag could not be added because it lacked an English name.");
    //                    continue;
    //                }
    //            }

    //            return RedirectToAction("Index", new { id = model.First().ParentId });
    //        }

    //        return RedirectToAction("Index");
    //    }
    }
}
