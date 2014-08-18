using ProjectLightSwitch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ProjectLightSwitch.Controllers
{
    public class StoryPortalController_ALT : Controller
    {
        StoryModel _db = new StoryModel();

        //
        // GET: /StoryPortal/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /StoryPortal/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /StoryPortal/Create

        public ActionResult Create()
        {
            StoryResponse model = new StoryResponse();
            return View(model);
        }

        //
        // POST: /StoryPortal/Create

        [HttpPost]
        public ActionResult Create([Bind(Include = "Age, Gender, CountryId")]StoryResponse storyResponse) //FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _db.StoryResponses.Add(storyResponse);
                    _db.SaveChanges();
                    return RedirectToAction("P2", new { id = storyResponse.StoryResponseId });
                }
                return View(); // What to do on failure?
                
            }
            catch
            {
                return View(); // Failure here?
            }
        }


        //
        // GET: /StoryPortal/P2/5

        [HttpGet]
        public ActionResult P2(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StoryResponse storyResponse = _db.StoryResponses.Find(id);
            if (storyResponse == null)
            {
                return HttpNotFound();
            }
            return View(storyResponse);
        }

        [HttpPost]
        public ActionResult P2([Bind(Include = "Story")] StoryResponse storyResponse)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _db.Entry(storyResponse).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    return RedirectToAction("P3", new { id = storyResponse.StoryResponseId });
                }
                return View();
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /StoryPortal/P3/5
        [HttpGet]
        public ActionResult P3(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StoryResponse storyResponse = _db.StoryResponses.Find(id);
            if (storyResponse == null)
            {
                return HttpNotFound();
            }
            return View(storyResponse);
        }

        [HttpPost]
        public ActionResult P3([Bind(Include = "Questions")] StoryResponse storyResponse)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _db.Entry(storyResponse).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View();
            }
            catch
            {
                return View();
            }
        }

        public void Dispose()
        {
            if (_db != null)
            {
                _db.Dispose();
            }
        }

        ////
        //// GET: /StoryPortal/Edit/5

        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        ////
        //// POST: /StoryPortal/Edit/5

        //[HttpPost]
        //public ActionResult Edit(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        ////
        //// GET: /StoryPortal/Delete/5

        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        ////
        //// POST: /StoryPortal/Delete/5

        //[HttpPost]
        //public ActionResult Delete(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
