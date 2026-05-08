using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BizzyQCU.Models.Admin;
using BizzyQCU.Models.Landingpage;

namespace BizzyQCU.Controllers
{
    public class AdminPanelController : Controller
    {
        private readonly AdminDb adminDb = new AdminDb();
        private readonly SimpleDb db = new SimpleDb();


        private bool IsAdmin()
        {
            return Session["UserId"] != null && Session["Role"]?.ToString() == "admin";
        }

        public ActionResult LandingAdmin()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            return View();
        }

        public ActionResult AdminlandingEntrep()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            return View();
        }

        public ActionResult AdminEditEntrep()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            return View();
        }

        public ActionResult AdminItemListing()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            return View();
        }

        public ActionResult AdminUsers()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Home");
            return View();
        }


        [HttpGet]
        public JsonResult GetAllUsers()
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Unauthorized" }, JsonRequestBehavior.AllowGet);

            var users = adminDb.GetAllUsers();
            return Json(users, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetPendingStudents()
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Unauthorized" }, JsonRequestBehavior.AllowGet);

            var students = adminDb.GetPendingStudentRequests();
            return Json(students, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetPendingEnterprises()
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Unauthorized" }, JsonRequestBehavior.AllowGet);

            var enterprises = adminDb.GetPendingEnterpriseRequests();
            return Json(enterprises, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult UpdateUserApproval(int userId, bool isApproved)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Unauthorized" });

            bool result = adminDb.UpdateUserApproval(userId, isApproved);
            return Json(new { success = result });
        }


        [HttpPost]
        public JsonResult DeleteUser(int userId)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Unauthorized" });

            bool result = adminDb.DeleteUser(userId);
            return Json(new { success = result });
        }


        [HttpPost]
        public JsonResult ApproveRequest(int requestId)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Unauthorized" });

            bool result = adminDb.ApproveRequest(requestId);
            return Json(new { success = result });
        }


        [HttpPost]
        public JsonResult RejectRequest(int requestId)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Unauthorized" });

            bool result = adminDb.RejectRequest(requestId);
            return Json(new { success = result });
        }


        [HttpGet]
        public JsonResult GetAllStudentRequests()
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Unauthorized" }, JsonRequestBehavior.AllowGet);

            var students = adminDb.GetAllStudentRequests();
            var json = Json(students, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        [HttpGet]
        public JsonResult GetAllEnterpriseRequests()
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Unauthorized" }, JsonRequestBehavior.AllowGet);

            var enterprises = adminDb.GetAllEnterpriseRequests();
            var json = Json(enterprises, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet]
        public JsonResult GetFeedbacks(int? rating = null)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Unauthorized" }, JsonRequestBehavior.AllowGet);

            var feedbacks = adminDb.GetFeedbacks(rating);
            return Json(feedbacks, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateStudentRequestDetails(int requestId, string username, string email, string firstname, string lastname, string studentNumber, string section, string contactNumber)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Unauthorized" });

            bool result = adminDb.UpdateStudentRequestDetails(requestId, username, email, firstname, lastname, studentNumber, section, contactNumber);
            return Json(new { success = result, message = result ? "Student details updated." : "Failed to update student details." });
        }

        [HttpPost]
        public JsonResult UpdateEnterpriseRequestDetails(int requestId, string username, string email, string storeName, string businessType, string contactNumber, string gcashNumber)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Unauthorized" });

            bool result = adminDb.UpdateEnterpriseRequestDetails(requestId, username, email, storeName, businessType, contactNumber, gcashNumber);
            return Json(new { success = result, message = result ? "Enterprise details updated." : "Failed to update enterprise details." });
        }

        [HttpPost]
        public JsonResult DeleteFeedback(int feedbackId)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Unauthorized" });

            bool result = adminDb.DeleteFeedback(feedbackId);
            return Json(new { success = result, message = result ? "Feedback deleted." : "Failed to delete feedback." });
        }

        [HttpGet]
        public JsonResult GetAllApprovedEnterprises()
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Unauthorized" }, JsonRequestBehavior.AllowGet);

            var enterprises = adminDb.GetAllApprovedEnterprises();
            return Json(enterprises, JsonRequestBehavior.AllowGet);
        }
    }
}