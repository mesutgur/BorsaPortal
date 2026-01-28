using System.Web.Mvc;

namespace BorsaPortal.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public abstract class AdminBaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.CurrentUser = User.Identity.Name;
            base.OnActionExecuting(filterContext);
        }

        protected void SetSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        protected void SetErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        protected void SetWarningMessage(string message)
        {
            TempData["WarningMessage"] = message;
        }

        protected void SetInfoMessage(string message)
        {
            TempData["InfoMessage"] = message;
        }
    }
}
