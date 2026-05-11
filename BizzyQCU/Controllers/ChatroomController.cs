using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using MySql.Data.MySqlClient;

namespace BizzyQCU.Controllers
{
    public class ChatroomController : Controller
    {
        string connStr = ConfigurationManager.ConnectionStrings["BizzyQCUConnection"].ConnectionString;

        // This matches the URL /Chatroom/Chatroom
        public ActionResult Chatroom()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            return View(); // This looks for Chatroom.cshtml
        }

        [HttpPost]
        public JsonResult SendMessage(string message)
        {
            if (Session["UserId"] == null) return Json(new { success = false });

            int senderId = (int)Session["UserId"];
            int receiverId = 1; // Default Enterprise ID for testing

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = "INSERT INTO chat_messages (sender_id, receiver_id, message_text) VALUES (@sid, @rid, @msg)";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@sid", senderId);
                    cmd.Parameters.AddWithValue("@rid", receiverId);
                    cmd.Parameters.AddWithValue("@msg", message);
                    cmd.ExecuteNonQuery();
                }
            }
            return Json(new { success = true });
        }

        [HttpGet]
        public JsonResult GetMessages()
        {
            int myId = Convert.ToInt32(Session["UserId"]);
            var list = new List<object>();

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = "SELECT sender_id, message_text FROM chat_messages WHERE (sender_id=@me OR receiver_id=@me) ORDER BY sent_at ASC";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@me", myId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new
                            {
                                senderId = reader["sender_id"],
                                text = reader["message_text"]
                            });
                        }
                    }
                }
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}