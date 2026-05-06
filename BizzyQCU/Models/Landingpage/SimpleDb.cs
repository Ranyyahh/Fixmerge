using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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
        public bool IsUsernameRequestExists(string username)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM approval_requests WHERE username = @username AND status = 'pending'";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
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

        public bool IsEmailRequestExists(string email)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM approval_requests WHERE email = @email AND status = 'pending'";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
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
            string studentNumber, string section, string contactNumber)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"INSERT INTO approval_requests 
                                   (username, password, email, role, firstname, lastname, student_number, section, contact_number, status) 
                                   VALUES (@username, @password, @email, 'student', @firstname, @lastname, @studentNumber, @section, @contact, 'pending')";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@firstname", firstName ?? "");
                        cmd.Parameters.AddWithValue("@lastname", lastName ?? "");
                        cmd.Parameters.AddWithValue("@studentNumber", studentNumber ?? "");
                        cmd.Parameters.AddWithValue("@section", section ?? "");
                        cmd.Parameters.AddWithValue("@contact", contactNumber ?? "");
                        cmd.ExecuteNonQuery();
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
            string contactNumber, string gcashNumber)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"INSERT INTO approval_requests 
                                   (username, password, email, role, store_name, store_description, contact_number, gcash_number, status) 
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SubmitEnterpriseRequest error: " + ex.Message);
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
