using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjectLightSwitch.Models;


namespace ProjectLightSwitch.Areas.SiteAdmin.Controllers
{
    public class StoryTypeController : Controller
    {
        //
        // GET: /SiteAdmin/Survey/
        // TODO: this might be better suited for a separate tag controller
        StoryModel sb = new StoryModel();
        public ActionResult Index()
        {
            
           // var context = TagSystem.GetTagsByType(showCategories: true);
            var model = sb.StoryTypes;
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
                using (var context = new StoryModel())
                {
                    var storyType = new StoryType();
                    context.StoryTypes.Add(storyType);
                    context.SaveChanges();
                    
                    foreach (var tag in model.SelectedTags)
                    {
                        storyType.StoryTypeTags.Add(new StoryTypeTag { StoryTypeId = storyType.StoryTypeId, TagId = tag });
                    }

                    var localizedStoryType = new LocalizedStoryType
                    {
                        Description = model.Description,
                        // TODO: Add real localization
                        LanguageId = Language.DefaultLanguageId, // model.LanguageId;
                        StoryTypeId = storyType.StoryTypeId,
                    };

                    context.LocalizedStoryTypes.Add(localizedStoryType);
                    context.SaveChanges();

                    foreach (var question in model.Questions)
                    {
                        localizedStoryType.Questions.Add(new Question { LocalizedStoryTypeId = localizedStoryType.LocalizedStoryTypeId, Prompt = question });
                    }
                    context.SaveChanges();
                }
                HelperFunctions.AddGlobalMessage(TempData, "Story Type Created");
                return RedirectToAction("Index");
            }
            HelperFunctions.AddGlobalMessage(TempData, "Error");
            return RedirectToAction("Index");
        }

        //public ActionResult Create([Bind(Include = "Description,QuestionText")] StoryType storyType)
        //{
        //    if (this.ModelState.IsValid)
        //    {
        //        using (var context = new StoryModel())
        //        {
        //            context.StoryTypes.Add(storyType);
        //            context.SaveChanges();
        //        }
        //    }

        //    return View();
        //}
        public ActionResult Details(int id)
        {
            var model = sb.StoryTypes.Find(id);
            return View(model);
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
        //        new Tag { EnglishText = Guid.NewGuid().ToString(), TagType = (byte)ProjectLightSwitch.Models.Enums.TagType.SelectableTag }, id);

        //    ViewBag.Message = "Ancestors Added";
        //    return RedirectToAction("View", new { id = category });
        //}



        public ActionResult RemoveTag(int category, int id)
        {
            bool result = TagSystem.RemoveTag(id);
            ViewBag.Message = result ? "Ancestors was deleted" : "Ancestors could not be deleted";
            return RedirectToAction("View", new { id = category });
        }
    }
}
