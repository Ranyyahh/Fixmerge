using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using BizzyQCU.Models.Admin; 

namespace BizzyQCU.Models.Landingpage
{
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Connection error: " + ex.Message);
                return false;
            }
        }

        // ========== SUBMIT FEEDBACK (ONLY ONE) ==========
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SubmitFeedback error: " + ex.Message);
                return false;
            }
        }

        // ========== CHECK EXISTING USERS (approved) ==========
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetUser error: " + ex.Message);
            }
            return null;
        }

        // ========== CHECK EXISTING REQUESTS (pending approval) ==========
        public bool IsUsernameRequestExists(string username, string role = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return false;
                }

                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM approval_requests WHERE username = @username AND LOWER(status) = 'pending'";
                    if (!string.IsNullOrWhiteSpace(role))
                    {
                        sql += " AND role = @role";
                    }
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        if (!string.IsNullOrWhiteSpace(role))
                        {
                            cmd.Parameters.AddWithValue("@role", role);
                        }
                        long count = (long)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("IsUsernameRequestExists error: " + ex.Message);
                return false;
            }
        }

        public bool IsEmailRequestExists(string email, string role = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return false;
                }

                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM approval_requests WHERE email = @email AND LOWER(status) = 'pending'";
                    if (!string.IsNullOrWhiteSpace(role))
                    {
                        sql += " AND role = @role";
                    }
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        if (!string.IsNullOrWhiteSpace(role))
                        {
                            cmd.Parameters.AddWithValue("@role", role);
                        }
                        long count = (long)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("IsEmailRequestExists error: " + ex.Message);
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetEnterpriseByUserId error: " + ex.Message);
            }
            return null;
        }

        // ========== GET STUDENT BY USER ID ==========
        public Students GetStudentByUserId(int userId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT student_id, user_id, firstname, lastname, student_number, section, contact_number FROM students WHERE user_id = @userId";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Students
                                {
                                    StudentId = reader.GetInt32("student_id"),
                                    UserId = reader.GetInt32("user_id"),
                                    Firstname = reader.GetString("firstname"),
                                    Lastname = reader.GetString("lastname"),
                                    StudentNumber = reader.GetString("student_number"),
                                    Section = reader.IsDBNull(reader.GetOrdinal("section")) ? "" : reader.GetString("section"),
                                    ContactNumber = reader.IsDBNull(reader.GetOrdinal("contact_number")) ? "" : reader.GetString("contact_number")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetStudentByUserId error: " + ex.Message);
            }
            return null;
        }

        // ========== SUBMIT STUDENT REQUEST ==========
        public bool SubmitStudentRequest(string firstName, string lastName, string username, string email, string password,
            string birthdate,
            string studentNumber, string section, string contactNumber, byte[] qcuIdBytes = null)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    try
                    {
                        string sqlWithQcuId = @"INSERT INTO approval_requests 
                                               (username, password, email, role, firstname, lastname, birthdate, student_number, section, contact_number, qcu_id, status) 
                                               VALUES (@username, @password, @email, 'student', @firstname, @lastname, @birthdate, @studentNumber, @section, @contact, @qcuId, 'pending')";

                        using (var cmd = new MySqlCommand(sqlWithQcuId, conn))
                        {
                            cmd.Parameters.AddWithValue("@username", username);
                            cmd.Parameters.AddWithValue("@password", password);
                            cmd.Parameters.AddWithValue("@email", email);
                            cmd.Parameters.AddWithValue("@firstname", firstName ?? "");
                            cmd.Parameters.AddWithValue("@lastname", lastName ?? "");
                            if (DateTime.TryParseExact(birthdate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedBirthdate) || DateTime.TryParse(birthdate, out parsedBirthdate))
                            {
                                cmd.Parameters.AddWithValue("@birthdate", parsedBirthdate.Date);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@birthdate", DBNull.Value);
                            }
                            cmd.Parameters.AddWithValue("@studentNumber", studentNumber ?? "");
                            cmd.Parameters.AddWithValue("@section", section ?? "");
                            cmd.Parameters.AddWithValue("@contact", contactNumber ?? "");
                            if (qcuIdBytes == null || qcuIdBytes.Length == 0)
                            {
                                cmd.Parameters.Add("@qcuId", MySqlDbType.Blob).Value = DBNull.Value;
                            }
                            else
                            {
                                cmd.Parameters.Add("@qcuId", MySqlDbType.Blob).Value = qcuIdBytes;
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (MySqlException ex) when (ex.Message.IndexOf("Unknown column 'qcu_id'", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // Backward-compatibility fallback for databases that do not yet have approval_requests.qcu_id.
                        string sqlWithoutQcuId = @"INSERT INTO approval_requests 
                                                  (username, password, email, role, firstname, lastname, birthdate, student_number, section, contact_number, status) 
                                                  VALUES (@username, @password, @email, 'student', @firstname, @lastname, @birthdate, @studentNumber, @section, @contact, 'pending')";

                        using (var fallbackCmd = new MySqlCommand(sqlWithoutQcuId, conn))
                        {
                            fallbackCmd.Parameters.AddWithValue("@username", username);
                            fallbackCmd.Parameters.AddWithValue("@password", password);
                            fallbackCmd.Parameters.AddWithValue("@email", email);
                            fallbackCmd.Parameters.AddWithValue("@firstname", firstName ?? "");
                            fallbackCmd.Parameters.AddWithValue("@lastname", lastName ?? "");
                            if (DateTime.TryParseExact(birthdate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedFallbackBirthdate) || DateTime.TryParse(birthdate, out parsedFallbackBirthdate))
                            {
                                fallbackCmd.Parameters.AddWithValue("@birthdate", parsedFallbackBirthdate.Date);
                            }
                            else
                            {
                                fallbackCmd.Parameters.AddWithValue("@birthdate", DBNull.Value);
                            }
                            fallbackCmd.Parameters.AddWithValue("@studentNumber", studentNumber ?? "");
                            fallbackCmd.Parameters.AddWithValue("@section", section ?? "");
                            fallbackCmd.Parameters.AddWithValue("@contact", contactNumber ?? "");
                            fallbackCmd.ExecuteNonQuery();
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SubmitStudentRequest error: " + ex.Message);
                return false;
            }
        }

        // ========== UPDATE ENTERPRISE PROFILE ==========
        public bool UpdateEnterpriseProfile(int userId, string storeName, string storeDescription, string contactNumber, string gcashNumber)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"UPDATE enterprises 
                           SET store_name = @storeName, 
                               store_description = @storeDesc, 
                               contact_number = @contact, 
                               gcash_number = @gcash 
                           WHERE user_id = @userId";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@storeName", storeName ?? "");
                        cmd.Parameters.AddWithValue("@storeDesc", storeDescription ?? "");
                        cmd.Parameters.AddWithValue("@contact", contactNumber ?? "");
                        cmd.Parameters.AddWithValue("@gcash", gcashNumber ?? "");
                        cmd.Parameters.AddWithValue("@userId", userId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateEnterpriseProfile error: " + ex.Message);
                return false;
            }
        }

        // ========== SUBMIT ENTERPRISE REQUEST ==========
        public bool SubmitEnterpriseRequest(string storeName, string enterpriseType, string username, string email, string password,
            string contactNumber, string gcashNumber, byte[] uploadedDocumentBytes = null)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    try
                    {
                        string sqlWithDoc = @"INSERT INTO approval_requests 
                                   (username, password, email, role, store_name, store_description, contact_number, gcash_number, uploaded_document, status) 
                                   VALUES (@username, @password, @email, 'enterprise', @storeName, @storeDesc, @contact, @gcash, @uploadedDocument, 'pending')";

                        using (var cmd = new MySqlCommand(sqlWithDoc, conn))
                        {
                            cmd.Parameters.AddWithValue("@username", username);
                            cmd.Parameters.AddWithValue("@password", password);
                            cmd.Parameters.AddWithValue("@email", email);
                            cmd.Parameters.AddWithValue("@storeName", storeName ?? "");
                            cmd.Parameters.AddWithValue("@storeDesc", enterpriseType ?? "");
                            cmd.Parameters.AddWithValue("@contact", contactNumber ?? "");
                            cmd.Parameters.AddWithValue("@gcash", gcashNumber ?? "");
                            if (uploadedDocumentBytes == null || uploadedDocumentBytes.Length == 0)
                            {
                                cmd.Parameters.Add("@uploadedDocument", MySqlDbType.Blob).Value = DBNull.Value;
                            }
                            else
                            {
                                cmd.Parameters.Add("@uploadedDocument", MySqlDbType.Blob).Value = uploadedDocumentBytes;
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (MySqlException ex) when (ex.Message.IndexOf("Unknown column 'uploaded_document'", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        string sqlWithoutDoc = @"INSERT INTO approval_requests 
                                   (username, password, email, role, store_name, store_description, contact_number, gcash_number, status) 
                                   VALUES (@username, @password, @email, 'enterprise', @storeName, @storeDesc, @contact, @gcash, 'pending')";

                        using (var fallbackCmd = new MySqlCommand(sqlWithoutDoc, conn))
                        {
                            fallbackCmd.Parameters.AddWithValue("@username", username);
                            fallbackCmd.Parameters.AddWithValue("@password", password);
                            fallbackCmd.Parameters.AddWithValue("@email", email);
                            fallbackCmd.Parameters.AddWithValue("@storeName", storeName ?? "");
                            fallbackCmd.Parameters.AddWithValue("@storeDesc", enterpriseType ?? "");
                            fallbackCmd.Parameters.AddWithValue("@contact", contactNumber ?? "");
                            fallbackCmd.Parameters.AddWithValue("@gcash", gcashNumber ?? "");
                            fallbackCmd.ExecuteNonQuery();
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SubmitEnterpriseRequest error: " + ex.Message);
                return false;
            }
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
                                   WHERE u.user_id = @userId
                                   LIMIT 1";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                return null;
                            }

                            var username = reader.IsDBNull(reader.GetOrdinal("username")) ? "" : reader.GetString("username");
                            var storeName = reader.IsDBNull(reader.GetOrdinal("store_name")) ? "" : reader.GetString("store_name");

                            return new StudentUserProfileData
                            {
                                Name = string.IsNullOrWhiteSpace(storeName) ? username : storeName,
                                ContactNumber = reader.IsDBNull(reader.GetOrdinal("contact_number")) ? "" : reader.GetString("contact_number"),
                                Email = reader.IsDBNull(reader.GetOrdinal("email")) ? "" : reader.GetString("email"),
                                Section = "",
                                StudentNumber = "",
                                PhotoDataUrl = ""
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetEnterpriseUserProfileByUserId error: " + ex.Message);
            }

            return null;
        }

        public bool UpdateEnterpriseUserProfile(int userId, string nameOrStoreName, string contactNumber, string email)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        string enterpriseSql = @"UPDATE enterprises
                                                 SET store_name = @storeName,
                                                     contact_number = @contact
                                                 WHERE user_id = @userId";

                        using (var entCmd = new MySqlCommand(enterpriseSql, conn, tx))
                        {
                            entCmd.Parameters.AddWithValue("@storeName", nameOrStoreName ?? "");
                            entCmd.Parameters.AddWithValue("@contact", contactNumber ?? "");
                            entCmd.Parameters.AddWithValue("@userId", userId);
                            entCmd.ExecuteNonQuery();
                        }

                        string userSql = @"UPDATE users
                                           SET email = @email
                                           WHERE user_id = @userId";

                        using (var userCmd = new MySqlCommand(userSql, conn, tx))
                        {
                            userCmd.Parameters.AddWithValue("@email", email ?? "");
                            userCmd.Parameters.AddWithValue("@userId", userId);
                            userCmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateEnterpriseUserProfile error: " + ex.Message);
                return false;
            }
        }

        public EnterpriseProfileData GetEnterpriseProfileDataByUserId(int userId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT e.enterprise_id, e.store_name, e.enterprise_type, e.gcash_number, u.email,
                                          e.store_logo, e.uploaded_document,
                                          em.manager_name, em.manager_student_id, em.manager_contact,
                                          COALESCE(NULLIF(em.manager_section, ''), s.section, '') AS manager_section
                                   FROM users u
                                   INNER JOIN enterprises e ON e.user_id = u.user_id
                                   LEFT JOIN enterprise_managers em ON em.enterprise_id = e.enterprise_id
                                   LEFT JOIN students s ON s.student_number = em.manager_student_id
                                   WHERE u.user_id = @userId
                                   LIMIT 1";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                return null;
                            }

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
                                Section = reader.IsDBNull(reader.GetOrdinal("manager_section")) ? "" : reader.GetString("manager_section"),
                                StoreLogoPath = reader.IsDBNull(reader.GetOrdinal("store_logo")) ? "" : reader.GetString("store_logo"),
                                QrDataUrl = reader.IsDBNull(reader.GetOrdinal("uploaded_document"))
                                    ? ""
                                    : "data:image/png;base64," + Convert.ToBase64String((byte[])reader["uploaded_document"])
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetEnterpriseProfileDataByUserId error: " + ex.Message);
                return null;
            }
        }

        public EnterpriseDashboardStatsData GetEnterpriseDashboardStatsByUserId(int userId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT
                                      (SELECT COUNT(*)
                                       FROM orders o
                                       WHERE o.enterprise_id = e.enterprise_id
                                         AND o.status = 'completed') AS orders_completed,
                                      (SELECT COUNT(*)
                                       FROM products p
                                       WHERE p.enterprise_id = e.enterprise_id
                                         AND p.status = 'active') AS products_listed,
                                      (SELECT COALESCE(SUM(th.amount), 0)
                                       FROM transaction_history th
                                       WHERE th.enterprise_id = e.enterprise_id
                                         AND th.transaction_type = 'sale') AS total_sales
                                   FROM enterprises e
                                   WHERE e.user_id = @userId
                                   LIMIT 1";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                return new EnterpriseDashboardStatsData();
                            }

                            return new EnterpriseDashboardStatsData
                            {
                                OrdersCompleted = reader.IsDBNull(reader.GetOrdinal("orders_completed")) ? 0 : Convert.ToInt32(reader["orders_completed"]),
                                ProductsListed = reader.IsDBNull(reader.GetOrdinal("products_listed")) ? 0 : Convert.ToInt32(reader["products_listed"]),
                                TotalSales = reader.IsDBNull(reader.GetOrdinal("total_sales")) ? 0m : Convert.ToDecimal(reader["total_sales"])
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetEnterpriseDashboardStatsByUserId error: " + ex.Message);
                return new EnterpriseDashboardStatsData();
            }
        }

        public bool SaveEnterpriseProfileData(int userId, string storeName, string enterpriseType, string gcashNumber, string email, string managerName, string managerStudentId, string managerContact, string section, string storeLogoPath, byte[] qrBytes)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        int enterpriseRowsAffected;
                        string enterpriseSql = @"UPDATE enterprises
                                                 SET store_name = @storeName,
                                                     enterprise_type = @enterpriseType,
                                                     gcash_number = @gcashNumber,
                                                     store_logo = COALESCE(@storeLogo, store_logo),
                                                     uploaded_document = COALESCE(@qrBlob, uploaded_document)
                                                 WHERE user_id = @userId";
                        using (var enterpriseCmd = new MySqlCommand(enterpriseSql, conn, tx))
                        {
                            enterpriseCmd.Parameters.AddWithValue("@storeName", storeName ?? "");
                            enterpriseCmd.Parameters.AddWithValue("@enterpriseType", enterpriseType ?? "");
                            enterpriseCmd.Parameters.AddWithValue("@gcashNumber", gcashNumber ?? "");
                            if (string.IsNullOrWhiteSpace(storeLogoPath))
                            {
                                enterpriseCmd.Parameters.AddWithValue("@storeLogo", DBNull.Value);
                            }
                            else
                            {
                                enterpriseCmd.Parameters.AddWithValue("@storeLogo", storeLogoPath);
                            }
                            if (qrBytes == null || qrBytes.Length == 0)
                            {
                                enterpriseCmd.Parameters.AddWithValue("@qrBlob", DBNull.Value);
                            }
                            else
                            {
                                enterpriseCmd.Parameters.Add("@qrBlob", MySqlDbType.Blob).Value = qrBytes;
                            }
                            enterpriseCmd.Parameters.AddWithValue("@userId", userId);
                            enterpriseRowsAffected = enterpriseCmd.ExecuteNonQuery();
                        }

                        if (enterpriseRowsAffected == 0)
                        {
                            string enterpriseInsertSql = @"INSERT INTO enterprises
                                                           (user_id, store_name, contact_number, enterprise_type, gcash_number, store_logo, uploaded_document, status)
                                                           VALUES (@userId, @storeName, @contactNumber, @enterpriseType, @gcashNumber, @storeLogo, @qrBlob, 'approved')";
                            using (var enterpriseInsertCmd = new MySqlCommand(enterpriseInsertSql, conn, tx))
                            {
                                enterpriseInsertCmd.Parameters.AddWithValue("@userId", userId);
                                enterpriseInsertCmd.Parameters.AddWithValue("@storeName", storeName ?? "");
                                enterpriseInsertCmd.Parameters.AddWithValue("@contactNumber", managerContact ?? "");
                                enterpriseInsertCmd.Parameters.AddWithValue("@enterpriseType", enterpriseType ?? "");
                                enterpriseInsertCmd.Parameters.AddWithValue("@gcashNumber", gcashNumber ?? "");
                                if (string.IsNullOrWhiteSpace(storeLogoPath))
                                {
                                    enterpriseInsertCmd.Parameters.AddWithValue("@storeLogo", DBNull.Value);
                                }
                                else
                                {
                                    enterpriseInsertCmd.Parameters.AddWithValue("@storeLogo", storeLogoPath);
                                }
                                if (qrBytes == null || qrBytes.Length == 0)
                                {
                                    enterpriseInsertCmd.Parameters.AddWithValue("@qrBlob", DBNull.Value);
                                }
                                else
                                {
                                    enterpriseInsertCmd.Parameters.Add("@qrBlob", MySqlDbType.Blob).Value = qrBytes;
                                }
                                enterpriseInsertCmd.ExecuteNonQuery();
                            }
                        }

                        int enterpriseId = 0;
                        string enterpriseIdSql = @"SELECT enterprise_id FROM enterprises WHERE user_id = @userId LIMIT 1";
                        using (var enterpriseIdCmd = new MySqlCommand(enterpriseIdSql, conn, tx))
                        {
                            enterpriseIdCmd.Parameters.AddWithValue("@userId", userId);
                            object result = enterpriseIdCmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                enterpriseId = Convert.ToInt32(result);
                            }
                        }

                        if (enterpriseId > 0)
                        {
                            string managerCountSql = "SELECT COUNT(*) FROM enterprise_managers WHERE enterprise_id = @enterpriseId";
                            long managerCount;
                            using (var managerCountCmd = new MySqlCommand(managerCountSql, conn, tx))
                            {
                                managerCountCmd.Parameters.AddWithValue("@enterpriseId", enterpriseId);
                                managerCount = Convert.ToInt64(managerCountCmd.ExecuteScalar());
                            }

                            if (managerCount > 0)
                            {
                                string managerUpdateSql = @"UPDATE enterprise_managers
                                                            SET manager_name = @managerName,
                                                                manager_student_id = @managerStudentId,
                                                                manager_contact = @managerContact,
                                                                manager_section = @managerSection
                                                            WHERE enterprise_id = @enterpriseId";
                                using (var managerUpdateCmd = new MySqlCommand(managerUpdateSql, conn, tx))
                                {
                                    managerUpdateCmd.Parameters.AddWithValue("@managerName", managerName ?? "");
                                    managerUpdateCmd.Parameters.AddWithValue("@managerStudentId", managerStudentId ?? "");
                                    managerUpdateCmd.Parameters.AddWithValue("@managerContact", managerContact ?? "");
                                    managerUpdateCmd.Parameters.AddWithValue("@managerSection", section ?? "");
                                    managerUpdateCmd.Parameters.AddWithValue("@enterpriseId", enterpriseId);
                                    managerUpdateCmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                string managerInsertSql = @"INSERT INTO enterprise_managers
                                                            (enterprise_id, manager_name, manager_student_id, manager_contact, manager_email, manager_section)
                                                            VALUES (@enterpriseId, @managerName, @managerStudentId, @managerContact, @managerEmail, @managerSection)";
                                using (var managerInsertCmd = new MySqlCommand(managerInsertSql, conn, tx))
                                {
                                    managerInsertCmd.Parameters.AddWithValue("@enterpriseId", enterpriseId);
                                    managerInsertCmd.Parameters.AddWithValue("@managerName", managerName ?? "");
                                    managerInsertCmd.Parameters.AddWithValue("@managerStudentId", managerStudentId ?? "");
                                    managerInsertCmd.Parameters.AddWithValue("@managerContact", managerContact ?? "");
                                    managerInsertCmd.Parameters.AddWithValue("@managerEmail", email ?? "");
                                    managerInsertCmd.Parameters.AddWithValue("@managerSection", section ?? "");
                                    managerInsertCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        string userSql = @"UPDATE users SET email = @email WHERE user_id = @userId";
                        using (var userCmd = new MySqlCommand(userSql, conn, tx))
                        {
                            userCmd.Parameters.AddWithValue("@email", email ?? "");
                            userCmd.Parameters.AddWithValue("@userId", userId);
                            userCmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SaveEnterpriseProfileData error: " + ex.Message);
                return false;
            }
        }

        public StudentUserProfileData GetStudentUserProfileByUserId(int userId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT u.username, u.email,
                                          s.firstname, s.lastname, s.contact_number, s.section, s.student_number, s.profile_image
                                   FROM users u
                                   LEFT JOIN students s ON s.user_id = u.user_id
                                   WHERE u.user_id = @userId
                                   LIMIT 1";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                return null;
                            }

                            var firstName = reader.IsDBNull(reader.GetOrdinal("firstname")) ? "" : reader.GetString("firstname");
                            var lastName = reader.IsDBNull(reader.GetOrdinal("lastname")) ? "" : reader.GetString("lastname");
                            var username = reader.IsDBNull(reader.GetOrdinal("username")) ? "" : reader.GetString("username");
                            string fullName = (firstName + " " + lastName).Trim();
                            if (string.IsNullOrWhiteSpace(fullName))
                            {
                                fullName = username;
                            }

                            byte[] profileImageBytes = null;
                            if (!reader.IsDBNull(reader.GetOrdinal("profile_image")))
                            {
                                profileImageBytes = (byte[])reader["profile_image"];
                            }

                            return new StudentUserProfileData
                            {
                                Name = fullName,
                                ContactNumber = reader.IsDBNull(reader.GetOrdinal("contact_number")) ? "" : reader.GetString("contact_number"),
                                Email = reader.IsDBNull(reader.GetOrdinal("email")) ? "" : reader.GetString("email"),
                                Section = reader.IsDBNull(reader.GetOrdinal("section")) ? "" : reader.GetString("section"),
                                StudentNumber = reader.IsDBNull(reader.GetOrdinal("student_number")) ? "" : reader.GetString("student_number"),
                                PhotoDataUrl = profileImageBytes != null && profileImageBytes.Length > 0
                                    ? "data:image/jpeg;base64," + Convert.ToBase64String(profileImageBytes)
                                    : ""
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetStudentUserProfileByUserId error: " + ex.Message);
            }

            return null;
        }

        public int GetUserIdByUsernameOrEmail(string usernameOrEmail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usernameOrEmail))
                {
                    return 0;
                }

                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT user_id 
                                   FROM users 
                                   WHERE username = @value OR email = @value
                                   LIMIT 1";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@value", usernameOrEmail);
                        object result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            return 0;
                        }

                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetUserIdByUsernameOrEmail error: " + ex.Message);
                return 0;
            }
        }

        public bool UpdateStudentUserProfile(int userId, string fullName, string contactNumber, string email, string photoDataUrl)
        {
            try
            {
                string firstName = "";
                string lastName = "";
                if (!string.IsNullOrWhiteSpace(fullName))
                {
                    var nameParts = fullName.Trim().Split(new[] { ' ' }, 2);
                    firstName = nameParts[0];
                    if (nameParts.Length > 1)
                    {
                        lastName = nameParts[1];
                    }
                }

                byte[] imageBytes = null;
                if (!string.IsNullOrWhiteSpace(photoDataUrl) && photoDataUrl.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
                {
                    int commaIndex = photoDataUrl.IndexOf(',');
                    if (commaIndex > -1 && commaIndex < photoDataUrl.Length - 1)
                    {
                        var base64 = photoDataUrl.Substring(commaIndex + 1).Replace(" ", "+");
                        imageBytes = Convert.FromBase64String(base64);
                    }
                }

                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        string existsSql = "SELECT COUNT(*) FROM students WHERE user_id = @userId";
                        long studentCount = 0;
                        using (var existsCmd = new MySqlCommand(existsSql, conn, tx))
                        {
                            existsCmd.Parameters.AddWithValue("@userId", userId);
                            studentCount = Convert.ToInt64(existsCmd.ExecuteScalar());
                        }

                        if (studentCount > 0)
                        {
                            string studentUpdateSql = @"UPDATE students
                                                        SET firstname = @firstname,
                                                            lastname = @lastname,
                                                            contact_number = @contactNumber
                                                            {0}
                                                        WHERE user_id = @userId";

                            var imageSetClause = imageBytes != null ? ", profile_image = @profileImage" : "";
                            studentUpdateSql = string.Format(studentUpdateSql, imageSetClause);

                            using (var studentUpdateCmd = new MySqlCommand(studentUpdateSql, conn, tx))
                            {
                                studentUpdateCmd.Parameters.AddWithValue("@firstname", firstName);
                                studentUpdateCmd.Parameters.AddWithValue("@lastname", lastName);
                                studentUpdateCmd.Parameters.AddWithValue("@contactNumber", contactNumber ?? "");
                                studentUpdateCmd.Parameters.AddWithValue("@userId", userId);
                                if (imageBytes != null)
                                {
                                    studentUpdateCmd.Parameters.Add("@profileImage", MySqlDbType.Blob).Value = imageBytes;
                                }
                                studentUpdateCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            string insertSql = @"INSERT INTO students
                                                 (user_id, firstname, lastname, student_number, section, contact_number, profile_image)
                                                 VALUES
                                                 (@userId, @firstname, @lastname, @studentNumber, @section, @contactNumber, @profileImage)";

                            string generatedStudentNumber = "TMP-" + userId.ToString("D6");

                            using (var insertCmd = new MySqlCommand(insertSql, conn, tx))
                            {
                                insertCmd.Parameters.AddWithValue("@userId", userId);
                                insertCmd.Parameters.AddWithValue("@firstname", firstName);
                                insertCmd.Parameters.AddWithValue("@lastname", lastName);
                                insertCmd.Parameters.AddWithValue("@studentNumber", generatedStudentNumber);
                                insertCmd.Parameters.AddWithValue("@section", "");
                                insertCmd.Parameters.AddWithValue("@contactNumber", contactNumber ?? "");
                                if (imageBytes != null)
                                {
                                    insertCmd.Parameters.Add("@profileImage", MySqlDbType.Blob).Value = imageBytes;
                                }
                                else
                                {
                                    insertCmd.Parameters.Add("@profileImage", MySqlDbType.Blob).Value = DBNull.Value;
                                }
                                insertCmd.ExecuteNonQuery();
                            }
                        }

                        string userSql = @"UPDATE users SET email = @email WHERE user_id = @userId";
                        using (var userCmd = new MySqlCommand(userSql, conn, tx))
                        {
                            userCmd.Parameters.AddWithValue("@email", email ?? "");
                            userCmd.Parameters.AddWithValue("@userId", userId);
                            userCmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateStudentUserProfile error: " + ex.Message);
                return false;
            }
        }
           
        }
    }


