using System;
using ProjectLightSwitch.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectLightSwitch.Areas.SiteAdmin.Controllers
{
    public class TagsController : Controller
    {
        //
        // GET: /SiteAdmin/Tags/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Edit(int id)
        {
            if (id == TagTree.InvisibleRootId)
            {
                return RedirectToAction("Index");
            }
            return View(TagSystem.GetTagEditOutputModel(id));
        }

        [HttpPost]
        public ActionResult ChangeName(TagEditInputModel model)
        {
            if (model.TagId == TagTree.InvisibleRootId)
            {
                return RedirectToAction("Index");
            }

            bool success = false;
            if (ModelState.IsValid)
            {
                success = true;
                foreach (var languageId in model.Names.Keys)
                {
                    success &= TagSystem.ChangeTagTranslation(model.TagId, languageId, model.Names[languageId]);
                }
            }
            string message = success 
                ? "The tag name(s) were changed."
                : "Not all tag names could be saved.";
            HelperFunctions.AddGlobalMessage(TempData, message);
            return RedirectToAction("Index", new { id = model.TagId });
        }

        [HttpPost]
        public ActionResult Remove(int tagId)
        {
            if (tagId == TagTree.InvisibleRootId)
            {
                return RedirectToAction("Index");
            }

            // Where to return after deletion
            var parent = TagSystem.GetParent(tagId);
            int returnId = (parent != null) ? parent.TagId : TagTree.InvisibleRootId;

            bool success = TagSystem.RemoveTag(tagId);
            
            string message = success 
                ? "The tag and its descendants were all removed." 
                : "An error occurred while attempting to delete this tag.";
            HelperFunctions.AddGlobalMessage(TempData, message);
            return RedirectToAction("Index", new { id = returnId });
        }

        [HttpPost]
        public ActionResult AddChildren(TagInputModel model)
        {
            bool success = false;
            if (ModelState.IsValid)
            {
                TagSystem.AddTag(model);
            }
            string message = success
                ? "Tags added"
                : "Some or all child tags could not be added.";
            HelperFunctions.AddGlobalMessage(TempData, message);
            return RedirectToAction("Index", new { id = model.ParentId });
        }
    }
}
