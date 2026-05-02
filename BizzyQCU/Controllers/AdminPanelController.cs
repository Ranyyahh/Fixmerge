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
            return Json(students, JsonRequestBehavior.AllowGet);
        }

     
        [HttpGet]
        public JsonResult GetAllEnterpriseRequests()
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Unauthorized" }, JsonRequestBehavior.AllowGet);

            var enterprises = adminDb.GetAllEnterpriseRequests();
            return Json(enterprises, JsonRequestBehavior.AllowGet);
        }
    }

}