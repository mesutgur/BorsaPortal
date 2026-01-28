using System.Web.Mvc;

namespace BorsaPortal.Web.Areas.Admin.Controllers
{
    public class DashboardController : AdminBaseController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
