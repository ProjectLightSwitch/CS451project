using System.Web.Mvc;

namespace ProjectLightSwitch.Areas.SiteAdmin
{
    public class SiteAdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "SiteAdmin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "SiteAdmin_default",
                "SiteAdmin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ProjectLightSwitch.Areas.SiteAdmin.Controllers" }
            );
        }
    }
}
