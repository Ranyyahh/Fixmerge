using System.Web.Mvc;
using BizzyQCU.Models.Landingpage;

namespace BizzyQCU.Controllers
{
    public class EditStoreController : Controller
    {
        private readonly SimpleDb db = new SimpleDb();

        public ActionResult EditStore()
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
            ViewBag.StoreDescription = enterprise.StoreDescription ?? "";
            ViewBag.ContactNumber = enterprise.ContactNumber ?? "";
            ViewBag.GcashNumber = enterprise.GcashNumber ?? "";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateStore(string storeName, string storeDescription, string contactNumber, string gcashNumber)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { success = false, message = "Please login first." });
            }

            int userId = (int)Session["UserId"];
            bool result = db.UpdateEnterpriseProfile(userId, storeName, storeDescription, contactNumber, gcashNumber);

            if (result)
            {
                return Json(new { success = true, message = "Store information updated successfully!" });
            }
            return Json(new { success = false, message = "Failed to update store information." });
        }
    }
}