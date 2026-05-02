using System.Web.Mvc;
using BizzyQCU.Models;

namespace BizzyQCU.Controllers
{
    public class EnterpriseDashboardController : Controller
    {
        private readonly SimpleDb db = new SimpleDb();

        public ActionResult EnterpriseDashboard()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            string role = Session["Role"]?.ToString();
            if (role != "enterprise")
            {
                return RedirectToAction("ProductList", "ProductList");
            }

            int userId = (int)Session["UserId"];
            var enterprise = db.GetEnterpriseByUserId(userId);

            if (enterprise == null)
            {
                return RedirectToAction("Login", "Home");
            }

            ViewBag.EnterpriseName = enterprise.StoreName;
            return View();
        }
    }
}