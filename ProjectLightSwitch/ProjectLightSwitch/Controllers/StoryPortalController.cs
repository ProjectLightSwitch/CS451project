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
        public ActionResult Browse([Bind(Include="SearchTerm,LanguageId")] StoryTypeResultsModel searchModel)
        {
            TagSystem.PopulateAvailableStoryTypes(searchModel);
            return View(searchModel);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="storyType">Actually the <see cref="LocalizedStoryType.LocalizedStoryTypeId" /> to search for</param>
        /// <returns></returns>
        public ActionResult Create(StoryResponseViewModel existingModel = null, int id = -1)
        {
            existingModel = existingModel ?? new StoryResponseViewModel();
            TagSystem.PopulateStoryResponseModelOutput(ref existingModel, id);
            return View(existingModel);
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
                TagSystem.PopulateStoryResponseModelOutput(ref model, model.StoryType.TranslatedStoryTypeId);
                return View(model);
            }
        }
    }
}
