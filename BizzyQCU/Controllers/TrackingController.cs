using BizzyQCU.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;

namespace BizzyQCU.Controllers
{
    public class TrackingController : Controller
    {
        public ActionResult Tracking(int orderId)
        {
            OrderTrackingViewModel model = null;
            string connStr = ConfigurationManager.ConnectionStrings["BizzyQCUConnection"].ConnectionString;

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();

                string sql = @"
                    SELECT o.order_id, o.status, o.total_amount, o.delivery_fee,
                           o.delivery_option, o.customer_location, o.order_note,
                           o.order_date, o.estimated_time, e.store_name
                    FROM orders o
                    INNER JOIN enterprises e ON o.enterprise_id = e.enterprise_id
                    WHERE o.order_id = @orderId";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = new OrderTrackingViewModel
                            {
                                OrderId = reader.GetInt32("order_id"),
                                Status = reader.IsDBNull(reader.GetOrdinal("status")) ? "preparing" : reader.GetString("status"),
                                TotalAmount = reader.GetDecimal("total_amount"),
                                DeliveryFee = reader.GetDecimal("delivery_fee"),
                                DeliveryOption = reader.GetString("delivery_option"),
                                CustomerLocation = reader.IsDBNull(reader.GetOrdinal("customer_location")) ? "" : reader.GetString("customer_location"),
                                OrderNote = reader.IsDBNull(reader.GetOrdinal("order_note")) ? "" : reader.GetString("order_note"),
                                OrderDate = reader.GetDateTime("order_date"),
                                EstimatedTime = reader.IsDBNull(reader.GetOrdinal("estimated_time")) ? "TBD" : reader.GetValue(reader.GetOrdinal("estimated_time")).ToString(),
                                StoreName = reader.GetString("store_name"),
                                Items = new List<OrderTrackingItem>()
                            };
                        }
                    }
                }

                if (model == null) { return HttpNotFound(); }

                string itemsSql = @"
                    SELECT p.product_name, oi.quantity, oi.unit_price
                    FROM order_items oi
                    INNER JOIN products p ON oi.product_id = p.product_id
                    WHERE oi.order_id = @orderId";

                using (var cmd2 = new MySqlCommand(itemsSql, conn))
                {
                    cmd2.Parameters.AddWithValue("@orderId", orderId);
                    using (var reader2 = cmd2.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            model.Items.Add(new OrderTrackingItem
                            {
                                ProductName = reader2.GetString("product_name"),
                                Quantity = reader2.GetInt32("quantity"),
                                UnitPrice = reader2.GetDecimal("unit_price")
                            });
                        }
                    }
                }
            }

            return View("Tracking", model);
        }

        public ActionResult History()
        {
            if (Session["UserId"] == null) { return RedirectToAction("Login", "Home"); }

            int userId = (int)Session["UserId"];
            var orders = new List<OrderHistoryItem>();
            string connStr = ConfigurationManager.ConnectionStrings["BizzyQCUConnection"].ConnectionString;

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = @"
                    SELECT o.order_id, o.status, o.total_amount, o.delivery_option,
                           o.order_date, e.store_name,
                           GROUP_CONCAT(CONCAT(oi.quantity, 'x ', p.product_name)
                               ORDER BY p.product_name SEPARATOR ', ') AS items_summary
                    FROM orders o
                    INNER JOIN enterprises e ON o.enterprise_id = e.enterprise_id
                    INNER JOIN students s ON o.student_id = s.student_id
                    LEFT JOIN order_items oi ON o.order_id = oi.order_id
                    LEFT JOIN products p ON oi.product_id = p.product_id
                    WHERE s.user_id = @userId
                    GROUP BY o.order_id, o.status, o.total_amount,
                             o.delivery_option, o.order_date, e.store_name
                    ORDER BY o.order_date DESC";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new OrderHistoryItem
                            {
                                OrderId = reader.GetInt32("order_id"),
                                Status = reader.IsDBNull(reader.GetOrdinal("status")) ? "preparing" : reader.GetString("status"),
                                TotalAmount = reader.GetDecimal("total_amount"),
                                DeliveryOption = reader.GetString("delivery_option"),
                                OrderDate = reader.GetDateTime("order_date"),
                                StoreName = reader.GetString("store_name"),
                                ItemsSummary = reader.IsDBNull(reader.GetOrdinal("items_summary")) ? "" : reader.GetString("items_summary")
                            });
                        }
                    }
                }
            }

            return View(orders);
        }
    }

    public class OrderHistoryItem
    {
        public int OrderId { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string DeliveryOption { get; set; }
        public DateTime OrderDate { get; set; }
        public string StoreName { get; set; }
        public string ItemsSummary { get; set; }
    }
}