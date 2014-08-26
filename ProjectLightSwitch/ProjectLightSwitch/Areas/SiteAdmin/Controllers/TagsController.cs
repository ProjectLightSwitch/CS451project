using System;
using ProjectLightSwitch.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectLightSwitch.Areas.SiteAdmin.Controllers
{
    [Authorize(Roles = SiteSettings.AdminRole)]
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
            if (id == TagTree.InvisibleRootId || TagSystem.GetTag(id) == null)
            {
                return RedirectToAction("Index");
            }
            return View(TagSystem.GetTagOutputViewModel(id));
        }

        [HttpPost]
        public ActionResult Rename(TagViewModel model)
        {
            if (
                model.Tag != null
                && !string.IsNullOrWhiteSpace(model.EnglishText)
                && model.Tag.TagId != TagTree.InvisibleRootId
                && ModelState.IsValid)
            {
                TagSystem.EditTag(model.Tag.TagId, model.TranslationsWithIntKeys);
                HelperFunctions.AddGlobalMessage(TempData, "Ancestor updated.");
            }
            else
            {
                HelperFunctions.AddGlobalMessage(TempData, "Error updating tag.");
            }
            return RedirectToAction("Edit", new { id = model.Tag != null ? model.Tag.TagId : 0 });
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

            if (parent == null)
            {
                HelperFunctions.AddGlobalMessage(TempData, "Ancestor doesn't exist");
                return RedirectToAction("Index");
            }

            bool success = TagSystem.RemoveTag(tagId);
            string message = success 
                ? "The tag and its descendants were all removed." 
                : "An error occurred while attempting to delete this tag.";
            HelperFunctions.AddGlobalMessage(TempData, message);
            return RedirectToAction("Index", new { id = parent.TagId });
        }

        [HttpPost]
        public ActionResult AddChildren(List<TagViewModel> model)
        {
            model.RemoveAll(m => string.IsNullOrWhiteSpace(m.EnglishText));
            int numAdded = (model.Count > 0) ? TagSystem.AddTags(model) : 0;
            string message = string.Format("{0} of {1} tags were added.", numAdded, model.Count);
            HelperFunctions.AddGlobalMessage(TempData, message);
            return RedirectToAction("Index", model.Count > 0 && model[0].Tag != null ? new { id = model[0].Tag.TagId } : null);
        }
    }

}
