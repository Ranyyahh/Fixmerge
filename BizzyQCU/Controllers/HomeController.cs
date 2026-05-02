using System;
using System.Web.Mvc;
using BizzyQCU.Models;

namespace BizzyQCU.Controllers
{
    public class HomeController : Controller
    {
        private readonly SimpleDb db = new SimpleDb();  // ADD THIS LINE

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult RegisterStudent()
        {
            return View();
        }

        public ActionResult RegisterEnterprise()
        {
            return View();
        }

        public ActionResult Tracking()
        {
            return View();
        }

        public ActionResult Manage()
        {
            return View();
        }

        public ActionResult History()
        {
            return View();
        }

        public ActionResult Homepage()
        {
     
            if (Session["UserId"] != null)
            {
                string role = Session["Role"]?.ToString();
                if (role == "enterprise")
                {
                    return RedirectToAction("EnterpriseDashboard", "EnterpriseDashboard");
                }
                else
                {
                    return RedirectToAction("ProductList", "ProductList");
                }
            }

            ViewBag.Message = "Your Homepage.";
            return View();
        }

        public ActionResult ProductList()
        {
      
            if (Session["UserId"] != null && Session["Role"]?.ToString() == "enterprise")
            {
                return RedirectToAction("EnterpriseDashboard", "EnterpriseDashboard");
            }

            ViewBag.Message = "Product List page.";
            return View();
        }

        public ActionResult ViewEnterprise()
        {
            if (Session["UserId"] != null && Session["Role"]?.ToString() == "enterprise")
            {
                return RedirectToAction("EnterpriseDashboard", "EnterpriseDashboard");
            }

            ViewBag.Message = "Your Enterprises.";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "About page.";
            return View();
        }

        public ActionResult UserProfile()
        {
            return RedirectToAction("UserProfile", "Profile");
        }

        public new ActionResult Profile()
        {
            return RedirectToAction("EnterpriseProfile", "Profile");
        }

        public ActionResult Index()
        {
            if (Session["UserId"] != null && Session["Role"]?.ToString() == "enterprise")
            {
                return RedirectToAction("EnterpriseDashboard", "EnterpriseDashboard");
            }
            return View("Homepage");
        }

        [HttpPost]
        public JsonResult SubmitFeedback(string email, string contactNumber, string userType, string category, string message, int rating)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return Json(new { success = false, message = "Please login first to submit feedback." });
                }

                int userId = (int)Session["UserId"];

                string userTypeValue = Session["Role"]?.ToString() == "enterprise" ? "entrepreneur" : "customer";

                bool result = db.SubmitFeedback(email, contactNumber, userTypeValue, category, message, rating, userId);

                if (result)
                {
                    return Json(new { success = true, message = "Feedback submitted successfully!" });
                }
                return Json(new { success = false, message = "Failed to submit feedback." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}