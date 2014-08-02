using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ProjectLightSwitch
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ProjectLightSwitch.Controllers" }
            );

            routes.MapRoute(
                name: "Json",
                url: "Json/{action}/{id}",
                defaults: new { controller = "Tags", id = UrlParameter.Optional }, 
                constraints: new { action = "(navigate|getchildren)" },
                namespaces: new[] { "ProjectLightSwitch.Controllers" }
            );

            // Hidden
            routes.MapRoute(
                name: "HiddenTags",
                url: "Tags/{*.}",
                defaults: new { controller = "Home", action = "Index"},
                namespaces: new[] { "ProjectLightSwitch.Controllers" }
            );


        }
    }
}