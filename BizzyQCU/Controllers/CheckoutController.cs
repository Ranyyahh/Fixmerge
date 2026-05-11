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
        // GET: Handles the Reorder logic
        public ActionResult Reorder(int orderId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserId"];
            string connStr = ConfigurationManager.ConnectionStrings["BizzyQCUConnection"].ConnectionString;

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();

                // 1. Clear the existing cart for this user first
                string clearSql = "DELETE FROM carts WHERE user_id = @uid";
                using (var cmd = new MySqlCommand(clearSql, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.ExecuteNonQuery();
                }

                // 2. Fetch items from the old order and the enterprise_id
                // We join with the orders table to get the enterprise_id for the cart
                string fetchSql = @"
            SELECT oi.product_id, oi.quantity, o.enterprise_id 
            FROM order_items oi
            JOIN orders o ON oi.order_id = o.order_id
            WHERE oi.order_id = @oid";

                using (var cmd = new MySqlCommand(fetchSql, conn))
                {
                    cmd.Parameters.AddWithValue("@oid", orderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        // We'll store them in a temporary list to avoid reader conflicts
                        var itemsToReorder = new System.Collections.Generic.List<dynamic>();
                        while (reader.Read())
                        {
                            itemsToReorder.Add(new
                            {
                                ProductId = reader["product_id"],
                                Quantity = reader["quantity"],
                                EnterpriseId = reader["enterprise_id"]
                            });
                        }
                        reader.Close();

                        // 3. Insert these items into the carts table
                        foreach (var item in itemsToReorder)
                        {
                            string insertCartSql = @"
                        INSERT INTO carts (user_id, product_id, quantity, enterprise_id) 
                        VALUES (@uid, @pid, @qty, @eid)";

                            using (var insCmd = new MySqlCommand(insertCartSql, conn))
                            {
                                insCmd.Parameters.AddWithValue("@uid", userId);
                                insCmd.Parameters.AddWithValue("@pid", item.ProductId);
                                insCmd.Parameters.AddWithValue("@qty", item.Quantity);
                                insCmd.Parameters.AddWithValue("@eid", item.EnterpriseId);
                                insCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }

            // 4. Redirect to the Checkout page (this matches your URL localhost:44343/Checkout/Checkout)
            return RedirectToAction("Checkout");
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