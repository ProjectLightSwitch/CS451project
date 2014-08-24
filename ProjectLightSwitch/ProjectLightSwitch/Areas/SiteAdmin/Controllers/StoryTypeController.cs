using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjectLightSwitch.Models;
using System.Transactions;


namespace ProjectLightSwitch.Areas.SiteAdmin.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class StoryTypeController : Controller
    {

        public ActionResult Index(StoryTypesViewModel model)
        {
            TagSystem.PopulateStoryTypesViewModel(model);
            return View(model);
        }

        public ActionResult Create()
        {
            var model = new StoryTypeCreationModel();
            using (var context = new StoryModel())
            {
                model.LanguageId = Language.DefaultLanguageId;
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(StoryTypeCreationModel model)
        {
            if (ModelState.IsValid)
            {
                var response = TagSystem.CreateStoryType(model);
                var message = response ?? "Story type created";
                HelperFunctions.AddGlobalMessage(TempData, message);
                return RedirectToAction("Index");
            }
            HelperFunctions.AddGlobalMessage(TempData, "Invalid input");
            return RedirectToAction("Index");
        }

        public ActionResult Details(int id)
        {
            var model = new LocalizedStoryTypeViewModel() { LocalizedStoryTypeId = id };
            TagSystem.PopulateLocalizedStoryTypeModel(model);
            if (model.LocalizedStoryType != null)
            {
                return View(model);
            }
            else
            {
                HelperFunctions.AddGlobalMessage(TempData, "An Invalid story type was supplied.");
                return RedirectToAction("Index");
            }
        }
        public ActionResult View(string search, int id = 0)
        {
            int languageId = Language.DefaultLanguageId;
            Tag tag = TagSystem.GetTag(id);
            int catId = (tag == null) ? 0 : tag.TagId;
            var paths = TagSystem.GetPaths(catId, search, true, languageId);
            ViewBag.Category = tag;
            return View(paths);
        }

        //public ActionResult AddChildTag(int category, int id = 0)
        //{
        //    // Hard coded for now
        //    TagSystem.AddTag(
        //        new Ancestor { EnglishText = Guid.NewGuid().ToString(), TagType = (byte)ProjectLightSwitch.Models.Enums.TagType.SelectableTag }, id);

        //    ViewBag.Message = "Descendants Added";
        //    return RedirectToAction("View", new { id = category });
        //}



        public ActionResult RemoveTag(int category, int id)
        {
            bool result = TagSystem.RemoveTag(id);
            ViewBag.Message = result ? "Descendants was deleted" : "Descendants could not be deleted";
            return RedirectToAction("View", new { id = category });
        }
    }
}
