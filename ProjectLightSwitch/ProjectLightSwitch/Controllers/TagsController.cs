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

        [AjaxOnly]
        public ActionResult GetCategories()
        {
            return Json(TagSystem.GetCategories(), JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public ActionResult GetChildTags(int parent)
        {
            return Json(TagSystem.GetChildTags(parent, false), JsonRequestBehavior.AllowGet);
        }

        [AjaxOnly]
        public ActionResult AddChildTag(int parentId, string text, byte type)
        {
            if(!Enum.IsDefined(typeof(TagType), type) || type == TagTree.InvisibleRootId)
            {
                return Json(new { error = "Invalid tag type." }); 
            }

            using (var context = new StoryModel())
            {
                var tagType = (TagType)type;


                TagSystem.AddTag(new Tag() { EnglishText = text, TagType = type }, parentId);

                var item = context.Tags.Where(t => t.TagId == type && t.TagId != TagTree.InvisibleRootId).FirstOrDefault();
                if (item != null)
                {
                    context.Tags.Remove(item);
                    bool success = context.SaveChanges() > 0;
                    return Json(new { result = success, error = success ? null : "There was an error removing the tag." });
                }
                return Json(new { error = "Ancestors not found." });
            }
        }

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
                return Json(new { error = "Ancestors not found." });
            }
        }
    }
}
