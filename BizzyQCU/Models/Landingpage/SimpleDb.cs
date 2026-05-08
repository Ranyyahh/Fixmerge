using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BizzyQCU.Models.Admin;

namespace BizzyQCU.Models.Landingpage
{
    // ========== MODEL CLASSES ==========
    public class StudentUserProfileData
    {
        public string Name { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string Section { get; set; }
        public string StudentNumber { get; set; }
        public string PhotoDataUrl { get; set; }
    }

    public class EnterpriseProfileData
    {
        public int EnterpriseId { get; set; }
        public string StoreName { get; set; }
        public string EnterpriseType { get; set; }
        public string GcashNumber { get; set; }
        public string Email { get; set; }
        public string ManagerName { get; set; }
        public string ManagerStudentId { get; set; }
        public string ManagerContact { get; set; }
        public string Section { get; set; }
        public string StoreLogoPath { get; set; }
        public string QrDataUrl { get; set; }
    }

    public class EnterpriseDashboardStatsData
    {
        public int OrdersCompleted { get; set; }
        public int ProductsListed { get; set; }
        public decimal TotalSales { get; set; }
    }

    public class EnterpriseDashboardStats
    {
        public decimal TotalSalesToday { get; set; }
        public int OrdersPending { get; set; }
        public int DeliveriesActive { get; set; }
        public int NewOrdersCount { get; set; }
    }

    public class WeeklySalesData
    {
        public string DayName { get; set; }
        public int DayOrder { get; set; }
        public decimal Sales { get; set; }
    }

    public class MonthlySalesData
    {
        public string MonthName { get; set; }
        public int MonthOrder { get; set; }
        public decimal Sales { get; set; }
    }

    // ========== ORDER MANAGEMENT MODELS ==========
    public class PendingOrder
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string DeliveryOption { get; set; }
        public string OrderNote { get; set; }
        public string Status { get; set; }
        public string OrderTime { get; set; }
        public string OrderDateFormatted { get; set; }
        public string Items { get; set; }
    }

    public class OrderDetails
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string DeliveryOption { get; set; }
        public string OrderNote { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string OrderTime { get; set; }
        public string OrderDateFormatted { get; set; }
        public string CustomerLocation { get; set; }
        public decimal DeliveryFee { get; set; }
        public List<OrderItem> Items { get; set; }
    }

    public class OrderItem
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;
    }

    // ========== SIMPLEDB CLASS ==========
    public class SimpleDb
    {
        private string connectionString = "server=localhost;database=BizzyQCU;uid=root;pwd=;";

        public bool TestConnection()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ========== GET ENTERPRISE BY USER ID ==========
        public Enterprises GetEnterpriseByUserId(int userId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT enterprise_id, user_id, store_name, store_description, contact_number, gcash_number, status FROM enterprises WHERE user_id = @userId";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Enterprises
                                {
                                    EnterpriseId = reader.GetInt32("enterprise_id"),
                                    UserId = reader.GetInt32("user_id"),
                                    StoreName = reader.GetString("store_name"),
                                    StoreDescription = reader.IsDBNull(reader.GetOrdinal("store_description")) ? "" : reader.GetString("store_description"),
                                    ContactNumber = reader.IsDBNull(reader.GetOrdinal("contact_number")) ? "" : reader.GetString("contact_number"),
                                    GcashNumber = reader.IsDBNull(reader.GetOrdinal("gcash_number")) ? "" : reader.GetString("gcash_number"),
                                    Status = reader.GetString("status")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        // ========== GET PENDING ORDERS ==========
        public List<PendingOrder> GetPendingOrders(int enterpriseId)
        {
            var orders = new List<PendingOrder>();
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                SELECT 
                    o.order_id,
                    CONCAT(COALESCE(s.firstname, ''), ' ', COALESCE(s.lastname, '')) AS customer_name,
                    o.total_amount,
                    o.order_date,
                    o.delivery_option,
                    o.order_note,
                    o.status,
                    DATE_FORMAT(o.order_date, '%h:%i %p') AS order_time,
                    DATE_FORMAT(o.order_date, '%M %d, %Y') AS order_date_formatted,
                    COALESCE((SELECT GROUP_CONCAT(CONCAT(oi.quantity, 'x ', p.product_name) SEPARATOR ', ') 
                     FROM order_items oi 
                     INNER JOIN products p ON p.product_id = oi.product_id 
                     WHERE oi.order_id = o.order_id), '') AS items
                FROM orders o
                INNER JOIN students s ON s.student_id = o.student_id
                WHERE o.enterprise_id = @enterpriseId 
                    AND o.status = 'preparing'
                ORDER BY o.order_date DESC";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@enterpriseId", enterpriseId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                orders.Add(new PendingOrder
                                {
                                    OrderId = reader.GetInt32("order_id"),
                                    CustomerName = reader.GetString("customer_name"),
                                    TotalAmount = reader.GetDecimal("total_amount"),
                                    OrderDate = reader.GetDateTime("order_date"),
                                    DeliveryOption = reader.IsDBNull(reader.GetOrdinal("delivery_option")) ? "pickup" : reader.GetString("delivery_option"),
                                    OrderNote = reader.IsDBNull(reader.GetOrdinal("order_note")) ? "" : reader.GetString("order_note"),
                                    Status = reader.GetString("status"),
                                    OrderTime = reader.GetString("order_time"),
                                    OrderDateFormatted = reader.GetString("order_date_formatted"),
                                    Items = reader.IsDBNull(reader.GetOrdinal("items")) ? "" : reader.GetString("items")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetPendingOrders error: " + ex.Message);
            }
            return orders;
        }

        // ========== UPDATE ORDER STATUS ==========
        public bool UpdateOrderStatus(int orderId, string status, int enterpriseId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE orders SET status = @status WHERE order_id = @orderId AND enterprise_id = @enterpriseId";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@status", status.ToLower());
                        cmd.Parameters.AddWithValue("@orderId", orderId);
                        cmd.Parameters.AddWithValue("@enterpriseId", enterpriseId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateOrderStatus error: " + ex.Message);
                return false;
            }
        }

        // ========== GET ORDER DETAILS ==========
        public OrderDetails GetOrderDetails(int orderId, int enterpriseId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT 
                            o.order_id,
                            CONCAT(COALESCE(s.firstname, ''), ' ', COALESCE(s.lastname, '')) AS customer_name,
                            s.contact_number AS customer_phone,
                            o.total_amount,
                            o.order_date,
                            o.delivery_option,
                            o.order_note,
                            o.status,
                            o.payment_method,
                            DATE_FORMAT(o.order_date, '%h:%i %p') AS order_time,
                            DATE_FORMAT(o.order_date, '%M %d, %Y') AS order_date_formatted,
                            o.customer_location,
                            o.delivery_fee
                        FROM orders o
                        INNER JOIN students s ON s.student_id = o.student_id
                        WHERE o.order_id = @orderId AND o.enterprise_id = @enterpriseId";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@orderId", orderId);
                        cmd.Parameters.AddWithValue("@enterpriseId", enterpriseId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var items = GetOrderItems(orderId);
                                return new OrderDetails
                                {
                                    OrderId = reader.GetInt32("order_id"),
                                    CustomerName = reader.GetString("customer_name"),
                                    CustomerPhone = reader.IsDBNull(reader.GetOrdinal("customer_phone")) ? "" : reader.GetString("customer_phone"),
                                    TotalAmount = reader.GetDecimal("total_amount"),
                                    OrderDate = reader.GetDateTime("order_date"),
                                    DeliveryOption = reader.IsDBNull(reader.GetOrdinal("delivery_option")) ? "pickup" : reader.GetString("delivery_option"),
                                    OrderNote = reader.IsDBNull(reader.GetOrdinal("order_note")) ? "" : reader.GetString("order_note"),
                                    Status = reader.GetString("status"),
                                    PaymentMethod = reader.IsDBNull(reader.GetOrdinal("payment_method")) ? "" : reader.GetString("payment_method"),
                                    OrderTime = reader.GetString("order_time"),
                                    OrderDateFormatted = reader.GetString("order_date_formatted"),
                                    CustomerLocation = reader.IsDBNull(reader.GetOrdinal("customer_location")) ? "" : reader.GetString("customer_location"),
                                    DeliveryFee = reader.IsDBNull(reader.GetOrdinal("delivery_fee")) ? 0 : reader.GetDecimal("delivery_fee"),
                                    Items = items
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetOrderDetails error: " + ex.Message);
                return null;
            }
            return null;
        }

        // ========== GET ORDER ITEMS ==========
        private List<OrderItem> GetOrderItems(int orderId)
        {
            var items = new List<OrderItem>();
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT 
                            p.product_name,
                            oi.quantity,
                            oi.unit_price
                        FROM order_items oi
                        INNER JOIN products p ON p.product_id = oi.product_id
                        WHERE oi.order_id = @orderId";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@orderId", orderId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new OrderItem
                                {
                                    ProductName = reader.GetString("product_name"),
                                    Quantity = reader.GetInt32("quantity"),
                                    UnitPrice = reader.GetDecimal("unit_price")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetOrderItems error: " + ex.Message);
            }
            return items;
        }

        // ========== DASHBOARD STATS METHODS ==========
        public decimal GetTotalSalesToday(int enterpriseId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT COALESCE(SUM(total_amount), 0) FROM orders 
                                   WHERE enterprise_id = @enterpriseId AND status = 'completed' AND DATE(order_date) = CURDATE()";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@enterpriseId", enterpriseId);
                        return Convert.ToDecimal(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public int GetPendingOrdersCount(int enterpriseId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT COUNT(*) FROM orders WHERE enterprise_id = @enterpriseId AND status = 'preparing'";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@enterpriseId", enterpriseId);
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public int GetActiveDeliveriesCount(int enterpriseId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT COUNT(*) FROM orders WHERE enterprise_id = @enterpriseId 
                                   AND delivery_option != 'pickup' AND status IN ('preparing', 'pending')";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@enterpriseId", enterpriseId);
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public int GetNewOrdersCountToday(int enterpriseId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT COUNT(*) FROM orders WHERE enterprise_id = @enterpriseId AND DATE(order_date) = CURDATE() AND status = 'pending'";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@enterpriseId", enterpriseId);
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public List<WeeklySalesData> GetWeeklySalesData(int userId)
        {
            var result = new List<WeeklySalesData>();
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string getEnterpriseSql = "SELECT enterprise_id FROM enterprises WHERE user_id = @userId LIMIT 1";
                    int enterpriseId = 0;
                    using (var cmd = new MySqlCommand(getEnterpriseSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        object obj = cmd.ExecuteScalar();
                        if (obj != null) enterpriseId = Convert.ToInt32(obj);
                    }
                    if (enterpriseId == 0) return GetDefaultWeeklyData();

                    string sql = @"SELECT DAYNAME(order_date) AS DayName, DAYOFWEEK(order_date) AS DayOrder, COALESCE(SUM(total_amount), 0) AS Sales
                                   FROM orders WHERE enterprise_id = @enterpriseId AND status = 'completed'
                                   AND order_date >= DATE_SUB(CURDATE(), INTERVAL 6 DAY)
                                   GROUP BY DAYNAME(order_date), DAYOFWEEK(order_date) ORDER BY DAYOFWEEK(order_date)";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@enterpriseId", enterpriseId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(new WeeklySalesData
                                {
                                    DayName = reader.GetString("DayName"),
                                    DayOrder = reader.GetInt32("DayOrder"),
                                    Sales = reader.GetDecimal("Sales")
                                });
                            }
                        }
                    }
                    result = FillMissingWeekDays(result);
                }
            }
            catch (Exception)
            {
                return GetDefaultWeeklyData();
            }
            return result;
        }

        public List<MonthlySalesData> GetMonthlySalesData(int userId)
        {
            var result = new List<MonthlySalesData>();
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string getEnterpriseSql = "SELECT enterprise_id FROM enterprises WHERE user_id = @userId LIMIT 1";
                    int enterpriseId = 0;
                    using (var cmd = new MySqlCommand(getEnterpriseSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        object obj = cmd.ExecuteScalar();
                        if (obj != null) enterpriseId = Convert.ToInt32(obj);
                    }
                    if (enterpriseId == 0) return GetDefaultMonthlyData();

                    string sql = @"SELECT DATE_FORMAT(order_date, '%b') AS MonthName, MONTH(order_date) AS MonthOrder, COALESCE(SUM(total_amount), 0) AS Sales
                                   FROM orders WHERE enterprise_id = @enterpriseId AND status = 'completed'
                                   AND order_date >= DATE_SUB(CURDATE(), INTERVAL 5 MONTH)
                                   GROUP BY DATE_FORMAT(order_date, '%b'), MONTH(order_date) ORDER BY MONTH(order_date)";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@enterpriseId", enterpriseId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(new MonthlySalesData
                                {
                                    MonthName = reader.GetString("MonthName"),
                                    MonthOrder = reader.GetInt32("MonthOrder"),
                                    Sales = reader.GetDecimal("Sales")
                                });
                            }
                        }
                    }
                    result = FillMissingMonths(result);
                }
            }
            catch (Exception)
            {
                return GetDefaultMonthlyData();
            }
            return result;
        }

        // ========== PRIVATE HELPER METHODS ==========
        private List<WeeklySalesData> FillMissingWeekDays(List<WeeklySalesData> data)
        {
            var allDays = new Dictionary<string, int> { { "Monday", 2 }, { "Tuesday", 3 }, { "Wednesday", 4 },
                { "Thursday", 5 }, { "Friday", 6 }, { "Saturday", 7 }, { "Sunday", 1 } };
            var result = new List<WeeklySalesData>();
            foreach (var day in allDays)
            {
                var existing = data.FirstOrDefault(x => x.DayName == day.Key);
                result.Add(existing ?? new WeeklySalesData { DayName = day.Key, DayOrder = day.Value, Sales = 0 });
            }
            return result.OrderBy(x => x.DayOrder).ToList();
        }

        private List<MonthlySalesData> FillMissingMonths(List<MonthlySalesData> data)
        {
            var allMonths = new Dictionary<string, int> { { "Jan", 1 }, { "Feb", 2 }, { "Mar", 3 }, { "Apr", 4 },
                { "May", 5 }, { "Jun", 6 }, { "Jul", 7 }, { "Aug", 8 }, { "Sep", 9 }, { "Oct", 10 }, { "Nov", 11 }, { "Dec", 12 } };
            var result = new List<MonthlySalesData>();
            int currentMonth = DateTime.Now.Month;
            int startMonth = currentMonth - 5;
            if (startMonth < 1) startMonth = 1;
            var last6Months = allMonths.Where(m => m.Value >= startMonth && m.Value <= currentMonth).OrderBy(m => m.Value).ToList();
            foreach (var month in last6Months)
            {
                var existing = data.FirstOrDefault(x => x.MonthName == month.Key);
                result.Add(existing ?? new MonthlySalesData { MonthName = month.Key, MonthOrder = month.Value, Sales = 0 });
            }
            return result.OrderBy(x => x.MonthOrder).ToList();
        }

        private List<WeeklySalesData> GetDefaultWeeklyData()
        {
            return new List<WeeklySalesData>
            {
                new WeeklySalesData { DayName = "Monday", DayOrder = 2, Sales = 0 },
                new WeeklySalesData { DayName = "Tuesday", DayOrder = 3, Sales = 0 },
                new WeeklySalesData { DayName = "Wednesday", DayOrder = 4, Sales = 0 },
                new WeeklySalesData { DayName = "Thursday", DayOrder = 5, Sales = 0 },
                new WeeklySalesData { DayName = "Friday", DayOrder = 6, Sales = 0 },
                new WeeklySalesData { DayName = "Saturday", DayOrder = 7, Sales = 0 },
                new WeeklySalesData { DayName = "Sunday", DayOrder = 1, Sales = 0 }
            }.OrderBy(x => x.DayOrder).ToList();
        }

        private List<MonthlySalesData> GetDefaultMonthlyData()
        {
            var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            var result = new List<MonthlySalesData>();
            for (int i = 0; i < months.Length; i++)
            {
                result.Add(new MonthlySalesData { MonthName = months[i], MonthOrder = i + 1, Sales = 0 });
            }
            int currentMonth = DateTime.Now.Month;
            return result.Where(m => m.MonthOrder >= currentMonth - 5 && m.MonthOrder <= currentMonth).OrderBy(m => m.MonthOrder).ToList();
        }

        // ========== SUBMIT FEEDBACK ==========
        public bool SubmitFeedback(string email, string contactNumber, string userType, string category, string message, int rating, int userId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"INSERT INTO feedbacks (user_id, user_type, email, contact_number, category, message, rating, status, created_at) 
                           VALUES (@userId, @userType, @email, @contact, @category, @message, @rating, 'pending', NOW())";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@userType", userType);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@contact", contactNumber ?? "");
                        cmd.Parameters.AddWithValue("@category", category ?? "");
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.Parameters.AddWithValue("@rating", rating);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ========== GET USER BY USERNAME OR EMAIL ==========
        public Users GetUserByUsernameOrEmail(string username)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT user_id, username, password, email, role, is_approved FROM users WHERE (username = @username OR email = @username) AND is_approved = 1";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Users
                                {
                                    UserId = reader.GetInt32("user_id"),
                                    Username = reader.GetString("username"),
                                    Password = reader.GetString("password"),
                                    Email = reader.GetString("email"),
                                    Role = reader.GetString("role"),
                                    IsApproved = reader.GetBoolean("is_approved")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        // ========== ADDITIONAL METHODS FOR PROFILE ==========
        public EnterpriseProfileData GetEnterpriseProfileDataByUserId(int userId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT e.enterprise_id, e.store_name, e.enterprise_type, e.gcash_number, u.email,
                                          em.manager_name, em.manager_student_id, em.manager_contact, em.manager_section
                                   FROM users u
                                   INNER JOIN enterprises e ON e.user_id = u.user_id
                                   LEFT JOIN enterprise_managers em ON em.enterprise_id = e.enterprise_id
                                   WHERE u.user_id = @userId
                                   LIMIT 1";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new EnterpriseProfileData
                                {
                                    EnterpriseId = reader.GetInt32("enterprise_id"),
                                    StoreName = reader.IsDBNull(reader.GetOrdinal("store_name")) ? "" : reader.GetString("store_name"),
                                    EnterpriseType = reader.IsDBNull(reader.GetOrdinal("enterprise_type")) ? "" : reader.GetString("enterprise_type"),
                                    GcashNumber = reader.IsDBNull(reader.GetOrdinal("gcash_number")) ? "" : reader.GetString("gcash_number"),
                                    Email = reader.IsDBNull(reader.GetOrdinal("email")) ? "" : reader.GetString("email"),
                                    ManagerName = reader.IsDBNull(reader.GetOrdinal("manager_name")) ? "" : reader.GetString("manager_name"),
                                    ManagerStudentId = reader.IsDBNull(reader.GetOrdinal("manager_student_id")) ? "" : reader.GetString("manager_student_id"),
                                    ManagerContact = reader.IsDBNull(reader.GetOrdinal("manager_contact")) ? "" : reader.GetString("manager_contact"),
                                    Section = reader.IsDBNull(reader.GetOrdinal("manager_section")) ? "" : reader.GetString("manager_section")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        public EnterpriseDashboardStatsData GetEnterpriseDashboardStatsByUserId(int userId)
        {
            var result = new EnterpriseDashboardStatsData();
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT COUNT(*) as orders_completed FROM orders o
                                   INNER JOIN enterprises e ON e.enterprise_id = o.enterprise_id
                                   WHERE e.user_id = @userId AND o.status = 'completed'";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        result.OrdersCompleted = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception)
            {
                return result;
            }
            return result;
        }

        public StudentUserProfileData GetEnterpriseUserProfileByUserId(int userId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT u.username, u.email, e.store_name, e.contact_number
                                   FROM users u
                                   LEFT JOIN enterprises e ON e.user_id = u.user_id
                                   WHERE u.user_id = @userId LIMIT 1";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new StudentUserProfileData
                                {
                                    Name = reader.IsDBNull(reader.GetOrdinal("store_name")) ? reader.GetString("username") : reader.GetString("store_name"),
                                    ContactNumber = reader.IsDBNull(reader.GetOrdinal("contact_number")) ? "" : reader.GetString("contact_number"),
                                    Email = reader.IsDBNull(reader.GetOrdinal("email")) ? "" : reader.GetString("email")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        public StudentUserProfileData GetStudentUserProfileByUserId(int userId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT u.username, u.email, s.firstname, s.lastname, s.contact_number, s.section, s.student_number
                                   FROM users u
                                   LEFT JOIN students s ON s.user_id = u.user_id
                                   WHERE u.user_id = @userId LIMIT 1";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string firstName = reader.IsDBNull(reader.GetOrdinal("firstname")) ? "" : reader.GetString("firstname");
                                string lastName = reader.IsDBNull(reader.GetOrdinal("lastname")) ? "" : reader.GetString("lastname");
                                string username = reader.GetString("username");
                                return new StudentUserProfileData
                                {
                                    Name = string.IsNullOrEmpty(firstName) ? username : $"{firstName} {lastName}",
                                    ContactNumber = reader.IsDBNull(reader.GetOrdinal("contact_number")) ? "" : reader.GetString("contact_number"),
                                    Email = reader.GetString("email"),
                                    Section = reader.IsDBNull(reader.GetOrdinal("section")) ? "" : reader.GetString("section"),
                                    StudentNumber = reader.IsDBNull(reader.GetOrdinal("student_number")) ? "" : reader.GetString("student_number")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        public bool IsUsernameRequestExists(string username, string role = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username)) return false;
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM approval_requests WHERE username = @username AND LOWER(status) = 'pending'";
                    if (!string.IsNullOrWhiteSpace(role)) sql += " AND role = @role";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        if (!string.IsNullOrWhiteSpace(role)) cmd.Parameters.AddWithValue("@role", role);
                        long count = (long)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception) { return false; }
        }

        public bool IsEmailRequestExists(string email, string role = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email)) return false;
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM approval_requests WHERE email = @email AND LOWER(status) = 'pending'";
                    if (!string.IsNullOrWhiteSpace(role)) sql += " AND role = @role";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        if (!string.IsNullOrWhiteSpace(role)) cmd.Parameters.AddWithValue("@role", role);
                        long count = (long)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception) { return false; }
        }

        public bool SubmitStudentRequest(string firstName, string lastName, string username, string email, string password, string birthdate, string studentNumber, string section, string contactNumber, byte[] qcuIdBytes = null)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"INSERT INTO approval_requests (username, password, email, role, firstname, lastname, birthdate, student_number, section, contact_number, status) 
                                   VALUES (@username, @password, @email, 'student', @firstname, @lastname, @birthdate, @studentNumber, @section, @contact, 'pending')";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@firstname", firstName ?? "");
                        cmd.Parameters.AddWithValue("@lastname", lastName ?? "");
                        cmd.Parameters.AddWithValue("@birthdate", string.IsNullOrEmpty(birthdate) ? DBNull.Value : (object)birthdate);
                        cmd.Parameters.AddWithValue("@studentNumber", studentNumber ?? "");
                        cmd.Parameters.AddWithValue("@section", section ?? "");
                        cmd.Parameters.AddWithValue("@contact", contactNumber ?? "");
                        cmd.ExecuteNonQuery();
                    }
                    return true;
                }
            }
            catch (Exception) { return false; }
        }

        public bool SubmitEnterpriseRequest(string storeName, string enterpriseType, string username, string email, string password, string contactNumber, string gcashNumber, byte[] uploadedDocumentBytes = null)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"INSERT INTO approval_requests (username, password, email, role, store_name, store_description, contact_number, gcash_number, status) 
                                   VALUES (@username, @password, @email, 'enterprise', @storeName, @storeDesc, @contact, @gcash, 'pending')";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@storeName", storeName ?? "");
                        cmd.Parameters.AddWithValue("@storeDesc", enterpriseType ?? "");
                        cmd.Parameters.AddWithValue("@contact", contactNumber ?? "");
                        cmd.Parameters.AddWithValue("@gcash", gcashNumber ?? "");
                        cmd.ExecuteNonQuery();
                    }
                    return true;
                }
            }
            catch (Exception) { return false; }
        }

        public bool UpdateEnterpriseProfile(int userId, string storeName, string storeDescription, string contactNumber, string gcashNumber)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"UPDATE enterprises SET store_name = @storeName, store_description = @storeDesc, contact_number = @contact, gcash_number = @gcash WHERE user_id = @userId";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@storeName", storeName ?? "");
                        cmd.Parameters.AddWithValue("@storeDesc", storeDescription ?? "");
                        cmd.Parameters.AddWithValue("@contact", contactNumber ?? "");
                        cmd.Parameters.AddWithValue("@gcash", gcashNumber ?? "");
                        cmd.Parameters.AddWithValue("@userId", userId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception) { return false; }
        }

        public bool UpdateEnterpriseUserProfile(int userId, string nameOrStoreName, string contactNumber, string email)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE enterprises SET store_name = @storeName, contact_number = @contact WHERE user_id = @userId";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@storeName", nameOrStoreName ?? "");
                        cmd.Parameters.AddWithValue("@contact", contactNumber ?? "");
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.ExecuteNonQuery();
                    }
                    string userSql = "UPDATE users SET email = @email WHERE user_id = @userId";
                    using (var cmd = new MySqlCommand(userSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email ?? "");
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.ExecuteNonQuery();
                    }
                    return true;
                }
            }
            catch (Exception) { return false; }
        }

        public bool UpdateStudentUserProfile(int userId, string fullName, string contactNumber, string email, string photoDataUrl)
        {
            try
            {
                string firstName = "", lastName = "";
                if (!string.IsNullOrWhiteSpace(fullName))
                {
                    var parts = fullName.Split(new[] { ' ' }, 2);
                    firstName = parts[0];
                    lastName = parts.Length > 1 ? parts[1] : "";
                }
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE students SET firstname = @firstname, lastname = @lastname, contact_number = @contact WHERE user_id = @userId";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@firstname", firstName);
                        cmd.Parameters.AddWithValue("@lastname", lastName);
                        cmd.Parameters.AddWithValue("@contact", contactNumber ?? "");
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.ExecuteNonQuery();
                    }
                    string userSql = "UPDATE users SET email = @email WHERE user_id = @userId";
                    using (var cmd = new MySqlCommand(userSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email ?? "");
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.ExecuteNonQuery();
                    }
                    return true;
                }
            }
            catch (Exception) { return false; }
        }

        public bool SaveEnterpriseProfileData(int userId, string storeName, string enterpriseType, string gcashNumber, string email, string managerName, string managerStudentId, string managerContact, string section, string storeLogoPath, byte[] qrBytes)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"UPDATE enterprises SET store_name = @storeName, enterprise_type = @enterpriseType, gcash_number = @gcashNumber WHERE user_id = @userId";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@storeName", storeName ?? "");
                        cmd.Parameters.AddWithValue("@enterpriseType", enterpriseType ?? "");
                        cmd.Parameters.AddWithValue("@gcashNumber", gcashNumber ?? "");
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.ExecuteNonQuery();
                    }
                    return true;
                }
            }
            catch (Exception) { return false; }
        }
    }
}