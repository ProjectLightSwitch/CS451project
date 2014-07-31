using System;
using ProjectLightSwitch.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectLightSwitch.Areas.SiteAdmin.Controllers
{

    public class TagAddInputModel
    {
        public int Parent {get;set;}
        IEnumerable<KeyValuePair<int, string>> TranslatedTags {get;set;}
    }


    public class TagsController : Controller
    {
        //
        // GET: /SiteAdmin/Tags/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Edit(Tag tag)
        {
            return View();
        }
    }
}
