﻿using System;
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
            var model = TagSystem.CreateStoryResponseModel(id);
            return View(model);
        }
    }
}
