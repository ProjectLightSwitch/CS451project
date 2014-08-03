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

            return View(TagSystem.GetTagEditViewModel(id));
        }



        [HttpPost]
        public ActionResult ChangeName(TagEditChangeNameInputModel model)
        {
            if (model.TagId == TagTree.InvisibleRootId)
            {
                return RedirectToAction("Index");
            }

            bool success = true;
            if (ModelState.IsValid)
            {
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

            var parent = TagSystem.GetParent(tagId);

            // Where to return after deletion
            int returnId = (parent != null) ? parent.TagId : TagTree.InvisibleRootId;
            bool success = TagSystem.RemoveTag(tagId);
            
            string message = success 
                ? "Tag and descendants were all deleted." 
                : "An error occurred while attempting to delete this tag.";
            HelperFunctions.AddGlobalMessage(TempData, message);
            return RedirectToAction("Index", new { id = returnId });
        }

        [HttpPost]
        public ActionResult AddChildren(TagEditAddChildrenInputModel model)
        {
            model.Children.RemoveAll(t => t.EnglishText == null || t.EnglishText.Length == 0);
            bool success = true;
            if (ModelState.IsValid)
            {
                foreach (var child in model.Children)
                {
                    success &= TagSystem.AddTag(child, model.ParentId);
                }
            }
            if (!success)
            {
                HelperFunctions.AddGlobalMessage(TempData, "Some or all child tags could not be added.");
            }
            return RedirectToAction("Index", new { id = model.ParentId });
        }
    }
}
