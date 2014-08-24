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

        [HttpGet]
        public ActionResult Browse([Bind(Include="SearchTerm,LanguageId")] StoryTypesViewModel searchModel)
        {
            TagSystem.PopulateStoryTypesViewModel(searchModel);
            return View(searchModel);
        }

        public ActionResult Search(StorySearchInputModel searchModel, int page = 0)
        {
            var model = new StorySearchResultsModel();
            model.StorySearchResults = TagSystem.GetStorySearchResults(searchModel, page, model.ResultsPerPage, model.RecentDays);
            return View(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storyType">Actually the <see cref="LocalizedStoryType.LocalizedStoryTypeId" /> to search for</param>
        /// <returns></returns>
        public ActionResult Create(int id = -1)
        {
            var model = new StoryResponseViewModel();
            model.StoryType.TranslatedStoryTypeId = id;
            TagSystem.PopulateStoryResponseModelOutput(ref model);
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(StoryResponseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var error = TagSystem.SaveStoryResponse(model);
                HelperFunctions.AddGlobalMessage(
                    TempData,
                    error ?? "Your story has been successfully saved.");
                return RedirectToAction("Index");
            }
            else
            {
                if (model == null || model.StoryType == null || model.StoryType.TranslatedStoryTypeId <= 0)
                {
                    return RedirectToAction("Index");
                }
                TagSystem.PopulateStoryResponseModelOutput(ref model);
                return View(model);
            }
        }
    }
}
