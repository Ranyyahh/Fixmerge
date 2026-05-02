using System;
using System.Web.Mvc;
using BizzyQCU.Models;
using System.Web.Script.Serialization;
using System.IO;

namespace BizzyQCU.Controllers
{
    public class AccountController : Controller
    {
        private readonly SimpleDb db = new SimpleDb();

        [HttpPost]
        public JsonResult Login(string username, string password)
        {
            try
            {
                if (!db.TestConnection())
                {
                    return Json(new { success = false, message = "Cannot connect to database. Please check if XAMPP MySQL is running." });
                }

                var user = db.GetUserByUsernameOrEmail(username);

                if (user == null)
                {
                    return Json(new { success = false, message = "Invalid username/email or password." });
                }

                if (user.Password != password)
                {
                    return Json(new { success = false, message = "Invalid username/email or password." });
                }

                if (!user.IsApproved)
                {
                    return Json(new { success = false, message = "Your account is pending approval. Please wait for admin confirmation." });
                }

                Session["UserId"] = user.UserId;
                Session["Username"] = user.Username;
                Session["Email"] = user.Email;
                Session["Role"] = user.Role;

                string redirectUrl = user.Role == "enterprise"
                    ? Url.Action("EnterpriseDashboard", "EnterpriseDashboard")
                    : Url.Action("ProductList", "ProductList");

                return Json(new
                {
                    success = true,
                    username = user.Username,
                    role = user.Role,
                    displayName = user.Username,
                    redirectUrl = redirectUrl
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server error: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult CheckLogin()
        {
            bool isLoggedIn = Session["UserId"] != null;
            return Json(new { isLoggedIn = isLoggedIn }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult RegisterStudent()
        {
            try
            {
                string jsonString = "";

                if (Request.Form.Count > 0)
                {
                    jsonString = Request.Form[0];
                }
                else if (Request.InputStream.Length > 0)
                {
                    Request.InputStream.Position = 0;
                    using (var reader = new StreamReader(Request.InputStream))
                    {
                        jsonString = reader.ReadToEnd();
                    }
                }

                if (string.IsNullOrEmpty(jsonString))
                {
                    return Json(new { success = false, message = "No data received." });
                }

                var serializer = new JavaScriptSerializer();
                var data = serializer.Deserialize<dynamic>(jsonString);

                string firstName = GetValue(data, "Firstname");
                string lastName = GetValue(data, "Lastname");
                string username = GetValue(data, "Username");
                string email = GetValue(data, "Email");
                string password = GetValue(data, "Password");
                string studentNumber = GetValue(data, "StudentNumber");
                string section = GetValue(data, "Section");
                string contactNumber = GetValue(data, "ContactNumber");

                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                    string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    return Json(new { success = false, message = "Please fill all required fields correctly." });
                }

                if (db.IsUsernameRequestExists(username))
                {
                    return Json(new { success = false, message = "Username already submitted for approval." });
                }

 
                if (db.IsEmailRequestExists(email))
                {
                    return Json(new { success = false, message = "Email already submitted for approval." });
                }

       
                bool result = db.SubmitStudentRequest(firstName, lastName, username, email, password, studentNumber, section, contactNumber);

                if (result)
                {
                    return Json(new { success = true, message = "Registration submitted! Please wait for admin approval." });
                }
                return Json(new { success = false, message = "Registration failed. Please try again." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult RegisterEnterprise()
        {
            try
            {
                string jsonString = "";

           
                if (Request.Form.Count > 0)
                {
                    jsonString = Request.Form[0];
                }
                else if (Request.InputStream.Length > 0)
                {
                    Request.InputStream.Position = 0;
                    using (var reader = new StreamReader(Request.InputStream))
                    {
                        jsonString = reader.ReadToEnd();
                    }
                }

                if (string.IsNullOrEmpty(jsonString))
                {
                    return Json(new { success = false, message = "No data received." });
                }

                var serializer = new JavaScriptSerializer();
                var data = serializer.Deserialize<dynamic>(jsonString);

                string storeName = GetValue(data, "StoreName");
                string enterpriseType = GetValue(data, "EnterpriseType");
                string username = GetValue(data, "Username");
                string email = GetValue(data, "Email");
                string password = GetValue(data, "Password");
                string contactNumber = GetValue(data, "ContactNumber");
                string gcashNumber = GetValue(data, "GcashNumber");

                // validate yung req field 
                if (string.IsNullOrEmpty(storeName) || string.IsNullOrEmpty(enterpriseType) ||
                    string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
                    string.IsNullOrEmpty(password) || string.IsNullOrEmpty(contactNumber))
                {
                    return Json(new { success = false, message = "Please fill all required fields correctly." });
                }

                // ichick ang usirnim
                if (db.IsUsernameRequestExists(username))
                {
                    return Json(new { success = false, message = "Username already submitted for approval." });
                }

                // icchik nya kung exist na ba ang email sa approval_requests
                if (db.IsEmailRequestExists(email))
                {
                    return Json(new { success = false, message = "Email already submitted for approval." });
                }

                // isubmit nia ean sa approval_requests
                bool result = db.SubmitEnterpriseRequest(storeName, enterpriseType, username, email, password, contactNumber, gcashNumber);

                if (result)
                {
                    return Json(new { success = true, message = "Registration submitted! Please wait for admin approval." });
                }
                return Json(new { success = false, message = "Registration failed. Please try again." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        private string GetValue(dynamic data, string key)
        {
            try
            {
                var dict = data as System.Collections.Generic.Dictionary<string, object>;
                if (dict != null && dict.ContainsKey(key))
                {
                    return dict[key]?.ToString() ?? "";
                }
                return "";
            }
            catch
            {
                return "";
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Home");
        }
    }
}