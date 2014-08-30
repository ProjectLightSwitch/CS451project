using System;
using ProjectLightSwitch.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjectLightSwitch.Attributes;

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

        public ActionResult Search(StoryResponseSearchInputModel searchModel)
        {
            var model = StorySystem.SearchStoryResponses(searchModel);
            return View(model);
        }

        public ActionResult View(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Search");
            }
            var model = StorySystem.GetStoryResponseDetails(id.Value);
            if (model == null)
            {
                HelperFunctions.AddGlobalMessage(TempData, "That story could not be found.");
                return RedirectToAction("Search");
            }
            return View(model);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="storyType">Actually the <see cref="LocalizedStoryType.LocalizedStoryTypeId" /> to search for</param>
        /// <returns></returns>
        public ActionResult Create(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return RedirectToAction("Browse");
            }

            var model = new StoryResponseCreationViewModel();
            StorySystem.PopulateStoryResponseModelOutput(id.Value, ref model);

            if (model.StoryTypeResultModel == null)
            { 
                HelperFunctions.AddGlobalMessage(TempData, "An invalid story type was specified.");
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(StoryResponseCreationViewModel model)
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

                var errors = ModelState.Values.SelectMany(v => v.Errors);

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
