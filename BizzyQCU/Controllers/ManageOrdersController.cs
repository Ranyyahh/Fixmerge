using System;
using System.Linq;
using System.Web.Mvc;
using BizzyQCU.Models.Landingpage;

namespace BizzyQCU.Controllers
{
    public class ManageOrdersController : Controller
    {
        private readonly SimpleDb db = new SimpleDb();

        public ActionResult ManageOrders()
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

            return View();
        }

        [HttpGet]
        public JsonResult GetPendingOrders()
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return Json(new { success = false, message = "Not logged in" }, JsonRequestBehavior.AllowGet);
                }

                int userId = (int)Session["UserId"];
                var enterprise = db.GetEnterpriseByUserId(userId);

                if (enterprise == null)
                {
                    return Json(new { success = false, message = "Enterprise not found" }, JsonRequestBehavior.AllowGet);
                }

                var orders = db.GetPendingOrders(enterprise.EnterpriseId);
                return Json(new { success = true, orders = orders }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return Json(new { success = false, message = "Not logged in" });
                }

                int userId = (int)Session["UserId"];
                var enterprise = db.GetEnterpriseByUserId(userId);

                if (enterprise == null)
                {
                    return Json(new { success = false, message = "Enterprise not found" });
                }

                bool result = db.UpdateOrderStatus(orderId, status, enterprise.EnterpriseId);
                return Json(new { success = result, message = result ? "Status updated" : "Update failed" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetOrderDetails(int orderId)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return Json(new { success = false, message = "Not logged in" }, JsonRequestBehavior.AllowGet);
                }

                int userId = (int)Session["UserId"];
                var enterprise = db.GetEnterpriseByUserId(userId);

                if (enterprise == null)
                {
                    return Json(new { success = false, message = "Enterprise not found" }, JsonRequestBehavior.AllowGet);
                }

                var order = db.GetOrderDetails(orderId, enterprise.EnterpriseId);
                return Json(new { success = true, order = order }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}