using System.Web.Mvc;
using BizzyQCU.Models;

namespace BizzyQCU.Controllers
{
    public class EnterpriseListController : Controller
    {
        public ActionResult EnterpriseList()
        {
            // Check if enterprise is logged in - redirect to their dashboard
            if (Session["UserId"] != null && Session["Role"]?.ToString() == "enterprise")
            {
                return RedirectToAction("EnterpriseDashboard", "EnterpriseDashboard");
            }

            return View();
        }
    }
}