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
            return RedirectToAction("Browse");
        }

        [HttpGet]
        public ActionResult Browse([Bind(Include="SearchTerm,LanguageId")] StoryTypesViewModel searchModel)
        {
            StorySystem.PopulateStoryTypesViewModel(searchModel);
            return View(searchModel);
        }

        public ActionResult Search(StorySearchInputModel searchModel, int page = 0)
        {
            var model = new StorySearchResultsModel();
            model.StorySearchResults = StorySystem.GetStorySearchResults(searchModel, page, model.ResultsPerPage, model.RecentDays);
            return View(model);
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="storyType">Actually the <see cref="LocalizedStoryType.LocalizedStoryTypeId" /> to search for</param>
        /// <returns></returns>
        public ActionResult Create(int id)
        {
            var model = new StoryResponseViewModel();
            StorySystem.PopulateStoryResponseModelOutput(id, ref model);

            if (model.StoryTypeResultModel == null)
            { 
                HelperFunctions.AddGlobalMessage(TempData, "An invalid story type was specified.");
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(StoryResponseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var error = StorySystem.SaveStoryResponse(model);
                HelperFunctions.AddGlobalMessage(
                    TempData,
                    error ?? "Your story has been successfully saved.");
                return RedirectToAction("Index");
            }
            else
            {
                if (model == null || model.StoryTypeResultModel == null || model.StoryTypeResultModel.TranslatedStoryTypeId <= 0)
                {
                    return RedirectToAction("Index");
                }
                StorySystem.PopulateStoryResponseModelOutput(model.StoryTypeResultModel.TranslatedStoryTypeId, ref model);
                return View(model);
            }
        }
    }
}
