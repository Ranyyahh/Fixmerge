using System;
using System.Configuration;
using System.Web.Mvc;
using MySql.Data.MySqlClient;

namespace BizzyQCU.Controllers
{
    public class CheckoutController : Controller
    {
        // GET: Show the checkout page
        public ActionResult Checkout()
        {
            return View("CheckoutPage");
        }

        // Backward-compatible redirect for old links/bookmarks
        public ActionResult CheckoutPage()
        {
            return RedirectToAction("Checkout");
        }

        // POST: Place the order — called via AJAX from Checkout.js
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(
            int enterpriseId,
            decimal totalAmount,
            string paymentMethod,
            string deliveryOption,
            string customerLocation,
            string orderNote,
            decimal deliveryFee,
            string itemsJson)   // JSON array: [{ productId, quantity, unitPrice }, ...]
        {
            // Must be logged in as a student
            if (Session["UserId"] == null)
            {
                return Json(new { success = false, message = "Please log in to place an order." });
            }

            int userId = (int)Session["UserId"];
            string connStr = ConfigurationManager.ConnectionStrings["BizzyQCUConnection"].ConnectionString;

            try
            {
                // Parse items JSON — simple manual parse kept C#5-compatible
                var items = Newtonsoft.Json.JsonConvert.DeserializeObject<CartItem[]>(itemsJson);
                if (items == null || items.Length == 0)
                {
                    return Json(new { success = false, message = "Your cart is empty." });
                }

                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    // Look up the student_id for this user
                    int studentId = 0;
                    string studentSql = "SELECT student_id FROM students WHERE user_id = @uid LIMIT 1";
                    using (var cmd = new MySqlCommand(studentSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        var result = cmd.ExecuteScalar();
                        if (result == null)
                        {
                            return Json(new { success = false, message = "Student profile not found." });
                        }
                        studentId = Convert.ToInt32(result);
                    }

                    // Insert the order
                    string orderSql = @"
                        INSERT INTO orders
                            (student_id, enterprise_id, total_amount, status,
                             payment_method, delivery_option, customer_location,
                             order_note, delivery_fee)
                        VALUES
                            (@studentId, @enterpriseId, @totalAmount, 'preparing',
                             @paymentMethod, @deliveryOption, @customerLocation,
                             @orderNote, @deliveryFee);
                        SELECT LAST_INSERT_ID();";

                    int newOrderId = 0;
                    using (var cmd = new MySqlCommand(orderSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@studentId", studentId);
                        cmd.Parameters.AddWithValue("@enterpriseId", enterpriseId);
                        cmd.Parameters.AddWithValue("@totalAmount", totalAmount);
                        cmd.Parameters.AddWithValue("@paymentMethod", paymentMethod ?? "cash");
                        cmd.Parameters.AddWithValue("@deliveryOption", deliveryOption ?? "pickup");
                        cmd.Parameters.AddWithValue("@customerLocation", customerLocation ?? "");
                        cmd.Parameters.AddWithValue("@orderNote", orderNote ?? "");
                        cmd.Parameters.AddWithValue("@deliveryFee", deliveryFee);
                        newOrderId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Insert order items
                    string itemSql = @"
                        INSERT INTO order_items (order_id, product_id, quantity, unit_price)
                        VALUES (@orderId, @productId, @qty, @unitPrice)";

                    foreach (var item in items)
                    {
                        using (var cmd = new MySqlCommand(itemSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@orderId", newOrderId);
                            cmd.Parameters.AddWithValue("@productId", item.ProductId);
                            cmd.Parameters.AddWithValue("@qty", item.Quantity);
                            cmd.Parameters.AddWithValue("@unitPrice", item.UnitPrice);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    return Json(new { success = true, orderId = newOrderId });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Order failed: " + ex.Message });
            }
        }

        // Helper class for deserializing cart items from the AJAX POST
        private class CartItem
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }
    }
}