using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using BizzyQCU.Models.Admin;
using BizzyQCU.Models.Landingpage;

namespace BizzyQCU.Models.Admin
{
    public class AdminDb
    {
        private string connectionString = "server=localhost;database=BizzyQCU;uid=root;pwd=;";

        // ========== GET ALL USERS ==========
        public List<Users> GetAllUsers()
        {
            var users = new List<Users>();
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT user_id, username, email, role, is_approved, created_at FROM users ORDER BY created_at DESC";
                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new Users
                            {
                                UserId = reader.GetInt32("user_id"),
                                Username = reader.GetString("username"),
                                Email = reader.GetString("email"),
                                Role = reader.GetString("role"),
                                IsApproved = reader.GetBoolean("is_approved"),
                                CreatedAt = reader.GetDateTime("created_at")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetAllUsers error: " + ex.Message);
            }
            return users;
        }

        public List<ApprovalRequests> GetAllStudentRequests()
        {
            var requests = new List<ApprovalRequests>();
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT ar.request_id, ar.username, ar.email, ar.role, ar.firstname, ar.lastname, 
                                  ar.student_number, ar.section, ar.contact_number, COALESCE(s.qcu_id, ar.qcu_id) AS qcu_id, ar.status, ar.submitted_at
                           FROM approval_requests ar
                           LEFT JOIN users u ON u.username = ar.username
                           LEFT JOIN students s ON s.user_id = u.user_id
                           WHERE ar.role = 'student'
                           ORDER BY 
                               CASE LOWER(ar.status)
                                   WHEN 'pending' THEN 1 
                                   WHEN 'approved' THEN 2 
                                   WHEN 'rejected' THEN 3 
                                   ELSE 4
                               END, 
                               ar.submitted_at DESC";
                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(new ApprovalRequests
                            {
                                RequestId = reader.GetInt32("request_id"),
                                Username = reader.GetString("username"),
                                Email = reader.GetString("email"),
                                Role = reader.GetString("role"),
                                Firstname = reader.IsDBNull(reader.GetOrdinal("firstname")) ? "" : reader.GetString("firstname"),
                                Lastname = reader.IsDBNull(reader.GetOrdinal("lastname")) ? "" : reader.GetString("lastname"),
                                StudentNumber = reader.IsDBNull(reader.GetOrdinal("student_number")) ? "" : reader.GetString("student_number"),
                                Section = reader.IsDBNull(reader.GetOrdinal("section")) ? "" : reader.GetString("section"),
                                ContactNumber = reader.IsDBNull(reader.GetOrdinal("contact_number")) ? "" : reader.GetString("contact_number"),
                                QcuId = reader.IsDBNull(reader.GetOrdinal("qcu_id")) ? null : (byte[])reader["qcu_id"],
                                Status = reader.GetString("status"),
                                SubmittedAt = reader.GetDateTime("submitted_at")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetAllStudentRequests error: " + ex.Message);
            }
            return requests;
        }


        public List<ApprovalRequests> GetAllEnterpriseRequests()
        {
            var requests = new List<ApprovalRequests>();
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT ar.request_id, ar.username, ar.email, ar.role, ar.store_name, ar.store_description, 
                                  ar.contact_number, ar.gcash_number, COALESCE(ar.uploaded_document, e.uploaded_document) AS uploaded_document, ar.status, ar.submitted_at
                           FROM approval_requests ar
                           LEFT JOIN users u ON u.username = ar.username
                           LEFT JOIN enterprises e ON e.user_id = u.user_id
                           WHERE ar.role = 'enterprise'
                           ORDER BY 
                               CASE LOWER(ar.status) 
                                   WHEN 'pending' THEN 1 
                                   WHEN 'approved' THEN 2 
                                   WHEN 'rejected' THEN 3 
                                   ELSE 4
                               END, 
                               ar.submitted_at DESC";
                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(new ApprovalRequests
                            {
                                RequestId = reader.GetInt32("request_id"),
                                Username = reader.GetString("username"),
                                Email = reader.GetString("email"),
                                Role = reader.GetString("role"),
                                StoreName = reader.IsDBNull(reader.GetOrdinal("store_name")) ? "" : reader.GetString("store_name"),
                                StoreDescription = reader.IsDBNull(reader.GetOrdinal("store_description")) ? "" : reader.GetString("store_description"),
                                ContactNumber = reader.IsDBNull(reader.GetOrdinal("contact_number")) ? "" : reader.GetString("contact_number"),
                                GcashNumber = reader.IsDBNull(reader.GetOrdinal("gcash_number")) ? "" : reader.GetString("gcash_number"),
                                UploadedDocument = reader.IsDBNull(reader.GetOrdinal("uploaded_document")) ? null : (byte[])reader["uploaded_document"],
                                Status = reader.GetString("status"),
                                SubmittedAt = reader.GetDateTime("submitted_at")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetAllEnterpriseRequests error: " + ex.Message);
            }
            return requests;
        }
        // ========== GET PENDING STUDENT APPROVALS ==========
        public List<ApprovalRequests> GetPendingStudentRequests()
        {
            var requests = new List<ApprovalRequests>();
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT request_id, username, email, role, firstname, lastname, 
                                          student_number, section, contact_number, qcu_id, status, submitted_at 
                                   FROM approval_requests 
                                   WHERE role = 'student' AND status = 'Pending' 
                                   ORDER BY submitted_at DESC";
                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(new ApprovalRequests
                            {
                                RequestId = reader.GetInt32("request_id"),
                                Username = reader.GetString("username"),
                                Email = reader.GetString("email"),
                                Role = reader.GetString("role"),
                                Firstname = reader.IsDBNull(reader.GetOrdinal("firstname")) ? "" : reader.GetString("firstname"),
                                Lastname = reader.IsDBNull(reader.GetOrdinal("lastname")) ? "" : reader.GetString("lastname"),
                                StudentNumber = reader.IsDBNull(reader.GetOrdinal("student_number")) ? "" : reader.GetString("student_number"),
                                Section = reader.IsDBNull(reader.GetOrdinal("section")) ? "" : reader.GetString("section"),
                                ContactNumber = reader.IsDBNull(reader.GetOrdinal("contact_number")) ? "" : reader.GetString("contact_number"),
                                QcuId = reader.IsDBNull(reader.GetOrdinal("qcu_id")) ? null : (byte[])reader["qcu_id"],
                                Status = reader.GetString("status"),
                                SubmittedAt = reader.GetDateTime("submitted_at")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetPendingStudentRequests error: " + ex.Message);
            }
            return requests;
        }


        public List<ApprovalRequests> GetPendingEnterpriseRequests()
        {
            var requests = new List<ApprovalRequests>();
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT request_id, username, email, role, store_name, store_description, 
                                          contact_number, gcash_number, status, submitted_at 
                                   FROM approval_requests 
                                   WHERE role = 'enterprise' AND status = 'Pending' 
                                   ORDER BY submitted_at DESC";
                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(new ApprovalRequests
                            {
                                RequestId = reader.GetInt32("request_id"),
                                Username = reader.GetString("username"),
                                Email = reader.GetString("email"),
                                Role = reader.GetString("role"),
                                StoreName = reader.IsDBNull(reader.GetOrdinal("store_name")) ? "" : reader.GetString("store_name"),
                                StoreDescription = reader.IsDBNull(reader.GetOrdinal("store_description")) ? "" : reader.GetString("store_description"),
                                ContactNumber = reader.IsDBNull(reader.GetOrdinal("contact_number")) ? "" : reader.GetString("contact_number"),
                                GcashNumber = reader.IsDBNull(reader.GetOrdinal("gcash_number")) ? "" : reader.GetString("gcash_number"),
                                Status = reader.GetString("status"),
                                SubmittedAt = reader.GetDateTime("submitted_at")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetPendingEnterpriseRequests error: " + ex.Message);
            }
            return requests;
        }

        //UPDATE USER APPROVAL 
        public bool UpdateUserApproval(int userId, bool isApproved)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE users SET is_approved = @isApproved WHERE user_id = @userId";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@isApproved", isApproved ? 1 : 0);
                        cmd.Parameters.AddWithValue("@userId", userId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateUserApproval error: " + ex.Message);
                return false;
            }
        }

        //DELETE USER 
        public bool DeleteUser(int userId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "DELETE FROM users WHERE user_id = @userId";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("DeleteUser error: " + ex.Message);
                return false;
            }
        }

        //APPROVE REQUEST
        //APPROVE REQUEST - WITH PROPER ERROR HANDLING
        public bool ApproveRequest(int requestId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Start a transaction para kung may error, mag-rollback lahat
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Step 1: Check if request exists and is pending
                            string checkSql = "SELECT status FROM approval_requests WHERE request_id = @requestId FOR UPDATE";
                            string currentStatus = null;
                            using (var cmd = new MySqlCommand(checkSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@requestId", requestId);
                                var result = cmd.ExecuteScalar();
                                if (result == null)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Request {requestId} not found");
                                    return false;
                                }
                                currentStatus = result.ToString();
                                if (currentStatus != "Pending")
                                {
                                    System.Diagnostics.Debug.WriteLine($"Request {requestId} is not pending. Current status: {currentStatus}");
                                    return false;
                                }
                            }

                            // Step 2: Get the request details
                            string selectSql = "SELECT * FROM approval_requests WHERE request_id = @requestId";
                            ApprovalRequests request = null;
                            using (var cmd = new MySqlCommand(selectSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@requestId", requestId);
                                using (var reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        request = new ApprovalRequests
                                        {
                                            Username = reader.GetString("username"),
                                            Password = reader.GetString("password"),
                                            Email = reader.GetString("email"),
                                            Role = reader.GetString("role"),
                                            Firstname = reader.IsDBNull(reader.GetOrdinal("firstname")) ? "" : reader.GetString("firstname"),
                                            Lastname = reader.IsDBNull(reader.GetOrdinal("lastname")) ? "" : reader.GetString("lastname"),
                                            Birthdate = reader.IsDBNull(reader.GetOrdinal("birthdate")) ? (DateTime?)null : reader.GetDateTime("birthdate"),
                                            StudentNumber = reader.IsDBNull(reader.GetOrdinal("student_number")) ? "" : reader.GetString("student_number"),
                                            Section = reader.IsDBNull(reader.GetOrdinal("section")) ? "" : reader.GetString("section"),
                                            ContactNumber = reader.IsDBNull(reader.GetOrdinal("contact_number")) ? "" : reader.GetString("contact_number"),
                                            QcuId = reader.IsDBNull(reader.GetOrdinal("qcu_id")) ? null : (byte[])reader["qcu_id"],
                                            StoreName = reader.IsDBNull(reader.GetOrdinal("store_name")) ? "" : reader.GetString("store_name"),
                                            StoreDescription = reader.IsDBNull(reader.GetOrdinal("store_description")) ? "" : reader.GetString("store_description"),
                                            GcashNumber = reader.IsDBNull(reader.GetOrdinal("gcash_number")) ? "" : reader.GetString("gcash_number")
                                        };
                                    }
                                }
                            }

                            if (request == null)
                            {
                                System.Diagnostics.Debug.WriteLine($"Request {requestId} not found after select");
                                return false;
                            }

                            System.Diagnostics.Debug.WriteLine($"Processing approval for {request.Role}: {request.Username}");

                            // Step 3: Insert into users table
                            string insertSql = @"INSERT INTO users (username, password, email, role, is_approved, created_at) 
                                         VALUES (@username, @password, @email, @role, 1, NOW())";
                            using (var cmd = new MySqlCommand(insertSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@username", request.Username);
                                cmd.Parameters.AddWithValue("@password", request.Password);
                                cmd.Parameters.AddWithValue("@email", request.Email);
                                cmd.Parameters.AddWithValue("@role", request.Role);
                                int rowsAffected = cmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Inserted into users, rows affected: {rowsAffected}");
                            }

                            // Step 4: Get the new user_id
                            int userId = 0;
                            string getUserIdSql = "SELECT user_id FROM users WHERE username = @username";
                            using (var cmd = new MySqlCommand(getUserIdSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@username", request.Username);
                                var result = cmd.ExecuteScalar();
                                if (result == null)
                                {
                                    throw new Exception("Failed to retrieve user_id after insert");
                                }
                                userId = Convert.ToInt32(result);
                                System.Diagnostics.Debug.WriteLine($"Created user_id: {userId}");
                            }

                            // Step 5: Insert into role-specific table
                            if (request.Role == "student")
                            {
                                string insertStudentSql = @"INSERT INTO students (user_id, firstname, lastname, birthdate, student_number, section, contact_number, qcu_id) 
                                                    VALUES (@userId, @firstname, @lastname, @birthdate, @studentNumber, @section, @contact, @qcuId)";
                                using (var cmd = new MySqlCommand(insertStudentSql, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@userId", userId);
                                    cmd.Parameters.AddWithValue("@firstname", request.Firstname ?? "");
                                    cmd.Parameters.AddWithValue("@lastname", request.Lastname ?? "");
                                    if (request.Birthdate.HasValue)
                                    {
                                        cmd.Parameters.AddWithValue("@birthdate", request.Birthdate.Value.Date);
                                    }
                                    else
                                    {
                                        cmd.Parameters.AddWithValue("@birthdate", DBNull.Value);
                                    }
                                    cmd.Parameters.AddWithValue("@studentNumber", request.StudentNumber ?? "");
                                    cmd.Parameters.AddWithValue("@section", request.Section ?? "");
                                    cmd.Parameters.AddWithValue("@contact", request.ContactNumber ?? "");
                                    if (request.QcuId == null || request.QcuId.Length == 0)
                                    {
                                        cmd.Parameters.Add("@qcuId", MySqlDbType.Blob).Value = DBNull.Value;
                                    }
                                    else
                                    {
                                        cmd.Parameters.Add("@qcuId", MySqlDbType.Blob).Value = request.QcuId;
                                    }
                                    int rowsAffected = cmd.ExecuteNonQuery();
                                    System.Diagnostics.Debug.WriteLine($"Inserted into students, rows affected: {rowsAffected}");
                                }
                            }
                            else if (request.Role == "enterprise")
                            {
                                string insertEnterpriseSql = @"INSERT INTO enterprises (user_id, store_name, store_description, contact_number, gcash_number, status) 
                                                       VALUES (@userId, @storeName, @storeDesc, @contact, @gcash, 'approved')";
                                using (var cmd = new MySqlCommand(insertEnterpriseSql, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@userId", userId);
                                    cmd.Parameters.AddWithValue("@storeName", request.StoreName ?? "");
                                    cmd.Parameters.AddWithValue("@storeDesc", request.StoreDescription ?? "");
                                    cmd.Parameters.AddWithValue("@contact", request.ContactNumber ?? "");
                                    cmd.Parameters.AddWithValue("@gcash", request.GcashNumber ?? "");
                                    int rowsAffected = cmd.ExecuteNonQuery();
                                    System.Diagnostics.Debug.WriteLine($"Inserted into enterprises, rows affected: {rowsAffected}");
                                }
                            }

                            // Step 6: Update approval_requests status
                            string updateSql = "UPDATE approval_requests SET status = 'Approved' WHERE request_id = @requestId";
                            using (var cmd = new MySqlCommand(updateSql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@requestId", requestId);
                                int rowsAffected = cmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Updated approval_requests status, rows affected: {rowsAffected}");
                            }

                            // Commit the transaction
                            transaction.Commit();
                            System.Diagnostics.Debug.WriteLine($"Request {requestId} approved successfully");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            // Rollback on error
                            transaction.Rollback();
                            System.Diagnostics.Debug.WriteLine($"Transaction rolled back. Error: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                            throw; // Re-throw to be caught by outer catch
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ApproveRequest error: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack trace: " + ex.StackTrace);
                return false;
            }
        }


        public bool RejectRequest(int requestId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();


                    string checkSql = "SELECT status FROM approval_requests WHERE request_id = @requestId";
                    string currentStatus = null;
                    using (var cmd = new MySqlCommand(checkSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@requestId", requestId);
                        var result = cmd.ExecuteScalar();
                        if (result == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Request {requestId} not found");
                            return false;
                        }
                        currentStatus = result.ToString();
                        if (currentStatus != "Pending")
                        {
                            System.Diagnostics.Debug.WriteLine($"Request {requestId} is not pending. Current status: {currentStatus}");
                            return false;
                        }
                    }

                    string sql = "UPDATE approval_requests SET status = 'Rejected' WHERE request_id = @requestId";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@requestId", requestId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        bool success = rowsAffected > 0;
                        System.Diagnostics.Debug.WriteLine($"RejectRequest: Request {requestId}, Rows affected: {rowsAffected}, Success: {success}");
                        return success;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("RejectRequest error: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack trace: " + ex.StackTrace);
                return false;
            }
        }

        public List<Feedback> GetFeedbacks(int? rating = null)
        {
            var feedbacks = new List<Feedback>();
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"SELECT feedback_id, user_id, user_type, email, contact_number, category, message, rating, status, created_at
                                   FROM feedbacks";

                    if (rating.HasValue)
                    {
                        sql += " WHERE rating = @rating";
                    }

                    sql += " ORDER BY feedback_id DESC";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        if (rating.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@rating", rating.Value);
                        }

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                feedbacks.Add(new Feedback
                                {
                                    FeedbackId = reader.GetInt32("feedback_id"),
                                    UserId = reader.GetInt32("user_id"),
                                    UserType = reader.GetString("user_type"),
                                    Email = reader.GetString("email"),
                                    ContactNumber = reader.IsDBNull(reader.GetOrdinal("contact_number")) ? "" : reader.GetString("contact_number"),
                                    Category = reader.IsDBNull(reader.GetOrdinal("category")) ? "" : reader.GetString("category"),
                                    Message = reader.GetString("message"),
                                    Rating = reader.GetInt32("rating"),
                                    Status = reader.GetString("status"),
                                    CreatedAt = reader.IsDBNull(reader.GetOrdinal("created_at")) ? (DateTime?)null : reader.GetDateTime("created_at")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetFeedbacks error: " + ex.Message);
            }

            return feedbacks;
        }

        public bool UpdateStudentRequestDetails(int requestId, string username, string email, string firstname, string lastname, string studentNumber, string section, string contactNumber)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        string requestSql = @"SELECT role, status, username FROM approval_requests 
                                              WHERE request_id = @requestId FOR UPDATE";
                        string role = null;
                        string status = null;
                        string oldUsername = null;
                        using (var requestCmd = new MySqlCommand(requestSql, conn, tx))
                        {
                            requestCmd.Parameters.AddWithValue("@requestId", requestId);
                            using (var reader = requestCmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    return false;
                                }
                                role = reader.GetString("role");
                                status = reader.GetString("status");
                                oldUsername = reader.GetString("username");
                            }
                        }

                        if (!string.Equals(role, "student", StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }

                        string updateRequestSql = @"UPDATE approval_requests
                                                    SET username = @username,
                                                        email = @email,
                                                        firstname = @firstname,
                                                        lastname = @lastname,
                                                        student_number = @studentNumber,
                                                        section = @section,
                                                        contact_number = @contactNumber
                                                    WHERE request_id = @requestId";
                        using (var cmd = new MySqlCommand(updateRequestSql, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@requestId", requestId);
                            cmd.Parameters.AddWithValue("@username", username ?? "");
                            cmd.Parameters.AddWithValue("@email", email ?? "");
                            cmd.Parameters.AddWithValue("@firstname", firstname ?? "");
                            cmd.Parameters.AddWithValue("@lastname", lastname ?? "");
                            cmd.Parameters.AddWithValue("@studentNumber", studentNumber ?? "");
                            cmd.Parameters.AddWithValue("@section", section ?? "");
                            cmd.Parameters.AddWithValue("@contactNumber", contactNumber ?? "");
                            cmd.ExecuteNonQuery();
                        }

                        if (string.Equals(status, "Approved", StringComparison.OrdinalIgnoreCase))
                        {
                            int userId = 0;
                            using (var userIdCmd = new MySqlCommand("SELECT user_id FROM users WHERE username = @oldUsername LIMIT 1", conn, tx))
                            {
                                userIdCmd.Parameters.AddWithValue("@oldUsername", oldUsername);
                                var userIdObj = userIdCmd.ExecuteScalar();
                                if (userIdObj != null && userIdObj != DBNull.Value)
                                {
                                    userId = Convert.ToInt32(userIdObj);
                                }
                            }

                            if (userId > 0)
                            {
                                using (var userCmd = new MySqlCommand("UPDATE users SET username = @username, email = @email WHERE user_id = @userId", conn, tx))
                                {
                                    userCmd.Parameters.AddWithValue("@username", username ?? "");
                                    userCmd.Parameters.AddWithValue("@email", email ?? "");
                                    userCmd.Parameters.AddWithValue("@userId", userId);
                                    userCmd.ExecuteNonQuery();
                                }

                                using (var studentCmd = new MySqlCommand(@"UPDATE students 
                                                                          SET firstname = @firstname,
                                                                              lastname = @lastname,
                                                                              student_number = @studentNumber,
                                                                              section = @section,
                                                                              contact_number = @contactNumber
                                                                          WHERE user_id = @userId", conn, tx))
                                {
                                    studentCmd.Parameters.AddWithValue("@firstname", firstname ?? "");
                                    studentCmd.Parameters.AddWithValue("@lastname", lastname ?? "");
                                    studentCmd.Parameters.AddWithValue("@studentNumber", studentNumber ?? "");
                                    studentCmd.Parameters.AddWithValue("@section", section ?? "");
                                    studentCmd.Parameters.AddWithValue("@contactNumber", contactNumber ?? "");
                                    studentCmd.Parameters.AddWithValue("@userId", userId);
                                    studentCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        tx.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateStudentRequestDetails error: " + ex.Message);
                return false;
            }
        }

        public bool UpdateEnterpriseRequestDetails(int requestId, string username, string email, string storeName, string businessType, string contactNumber, string gcashNumber)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        string requestSql = @"SELECT role, status, username FROM approval_requests 
                                              WHERE request_id = @requestId FOR UPDATE";
                        string role = null;
                        string status = null;
                        string oldUsername = null;
                        using (var requestCmd = new MySqlCommand(requestSql, conn, tx))
                        {
                            requestCmd.Parameters.AddWithValue("@requestId", requestId);
                            using (var reader = requestCmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    return false;
                                }
                                role = reader.GetString("role");
                                status = reader.GetString("status");
                                oldUsername = reader.GetString("username");
                            }
                        }

                        if (!string.Equals(role, "enterprise", StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }

                        string updateRequestSql = @"UPDATE approval_requests
                                                    SET username = @username,
                                                        email = @email,
                                                        store_name = @storeName,
                                                        store_description = @businessType,
                                                        contact_number = @contactNumber,
                                                        gcash_number = @gcashNumber
                                                    WHERE request_id = @requestId";
                        using (var cmd = new MySqlCommand(updateRequestSql, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@requestId", requestId);
                            cmd.Parameters.AddWithValue("@username", username ?? "");
                            cmd.Parameters.AddWithValue("@email", email ?? "");
                            cmd.Parameters.AddWithValue("@storeName", storeName ?? "");
                            cmd.Parameters.AddWithValue("@businessType", businessType ?? "");
                            cmd.Parameters.AddWithValue("@contactNumber", contactNumber ?? "");
                            cmd.Parameters.AddWithValue("@gcashNumber", gcashNumber ?? "");
                            cmd.ExecuteNonQuery();
                        }

                        if (string.Equals(status, "Approved", StringComparison.OrdinalIgnoreCase))
                        {
                            int userId = 0;
                            using (var userIdCmd = new MySqlCommand("SELECT user_id FROM users WHERE username = @oldUsername LIMIT 1", conn, tx))
                            {
                                userIdCmd.Parameters.AddWithValue("@oldUsername", oldUsername);
                                var userIdObj = userIdCmd.ExecuteScalar();
                                if (userIdObj != null && userIdObj != DBNull.Value)
                                {
                                    userId = Convert.ToInt32(userIdObj);
                                }
                            }

                            if (userId > 0)
                            {
                                using (var userCmd = new MySqlCommand("UPDATE users SET username = @username, email = @email WHERE user_id = @userId", conn, tx))
                                {
                                    userCmd.Parameters.AddWithValue("@username", username ?? "");
                                    userCmd.Parameters.AddWithValue("@email", email ?? "");
                                    userCmd.Parameters.AddWithValue("@userId", userId);
                                    userCmd.ExecuteNonQuery();
                                }

                                using (var enterpriseCmd = new MySqlCommand(@"UPDATE enterprises 
                                                                             SET store_name = @storeName,
                                                                                 store_description = @businessType,
                                                                                 contact_number = @contactNumber,
                                                                                 gcash_number = @gcashNumber
                                                                             WHERE user_id = @userId", conn, tx))
                                {
                                    enterpriseCmd.Parameters.AddWithValue("@storeName", storeName ?? "");
                                    enterpriseCmd.Parameters.AddWithValue("@businessType", businessType ?? "");
                                    enterpriseCmd.Parameters.AddWithValue("@contactNumber", contactNumber ?? "");
                                    enterpriseCmd.Parameters.AddWithValue("@gcashNumber", gcashNumber ?? "");
                                    enterpriseCmd.Parameters.AddWithValue("@userId", userId);
                                    enterpriseCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        tx.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateEnterpriseRequestDetails error: " + ex.Message);
                return false;
            }
        }

        public bool DeleteFeedback(int feedbackId)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("DELETE FROM feedbacks WHERE feedback_id = @feedbackId", conn))
                    {
                        cmd.Parameters.AddWithValue("@feedbackId", feedbackId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("DeleteFeedback error: " + ex.Message);
                return false;
            }
        }
    }
}

