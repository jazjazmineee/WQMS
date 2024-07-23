using MySql.Data.MySqlClient;
using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace WaterGuard_2024
{
    public partial class LoginPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ValidationSettings.UnobtrusiveValidationMode = UnobtrusiveValidationMode.None;

        }
        private string GetConnectionString()
        {
            return "Server=tcp:waterguard.database.windows.net,1433;Initial Catalog=waterqualitymonitoring;Persist Security Info=False;User ID=waterguardserver;Password=K7BVJwaterguard$;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }

        protected void LoginBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(unametxtbox.Text) || !string.IsNullOrEmpty(pwordtxtbox.Text))
            {
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                {
                    try
                    {
                        connection.Open();

                        // Get the username and password from the TextBoxes
                        string enteredUsername = unametxtbox.Text;
                        string enteredPassword = pwordtxtbox.Text;

                        // Perform database operations here
                        string query = "SELECT * FROM accounts WHERE Username = @username";
                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@username", enteredUsername);

                        // Execute the query
                        SqlDataReader reader = cmd.ExecuteReader();

                        // Check if the reader has rows
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                string storedPassword = reader["Password"].ToString();
                                string role = reader["UserRole"].ToString();
                                string status = reader["Status"].ToString();
                                string userID = reader["UserID"].ToString();

                                if (enteredPassword == storedPassword)
                                {
                                    if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (role.Equals("Owner", StringComparison.OrdinalIgnoreCase) || role.Equals("Manager", StringComparison.OrdinalIgnoreCase) || role.Equals("Staff", StringComparison.OrdinalIgnoreCase))
                                        {
                                            // Set the user role in the session
                                            Session["UserRole"] = role;

                                            // Add the following line to set the authentication cookie
                                            FormsAuthentication.SetAuthCookie(enteredUsername, false);

                                            // Set the user name in the session
                                            Session["Name"] = reader["Name"].ToString(); // Replace "Name" with the actual column name in your database
                                            Session["Username"] = reader["Username"].ToString();
                                            Session["Contact"] = reader["Contact"].ToString();
                                            Session["UserID"] = reader["UserID"].ToString();

                                            //Add to Audit
                                            LogAudit(userID, "Login", "Successful");

                                            // Redirect to the DashboardPage
                                            Response.Redirect("Dashboard.aspx");
                                        }
                                    }
                                    else
                                    {
                                        // Username is not active
                                        error_alert.InnerText = "Username is not active."; // Change the text accordingly
                                        error_alert.Visible = true;
                                        ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", "showAlertAndHide();", true);

                                        //Add to Audit
                                        LogAudit(reader["UserID"].ToString(), "Login", "Unsuccessful");
                                    }
                                }
                                else
                                {
                                    // Invalid password
                                    error_alert.InnerText = "Invalid password."; // Change the text accordingly
                                    error_alert.Visible = true;
                                    ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", "showAlertAndHide();", true);

                                    //Add to Audit
                                    LogAudit(userID, "Login", "Unsuccessful");
                                }
                            }
                        }
                        else
                        {
                            // Invalid username
                            error_alert.InnerText = "Invalid username."; // Change the text accordingly
                            error_alert.Visible = true;
                            ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", "showAlertAndHide();", true);

                            //Add to Audit
                            LogAudit(enteredUsername, "Login", "Unsuccessful");
                        }

                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions
                        ScriptManager.RegisterStartupScript(this, GetType(), "showalert", $"alert('Error: {ex.Message}');", true);
                    }
                }
            }
            else
            {
                // Empty textboxes
                error_alert.InnerText = "Username or password is invalid."; // Change the text accordingly
                error_alert.Visible = true;
                ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", "showAlertAndHide();", true);
            }
        }

        private void LogAudit(string userid, string activity, string status)
        {
            string connectionString = GetConnectionString();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string auditQuery = "INSERT INTO AuditReport (UserID, DateTime, Activity, Status) VALUES (@UserID, GETDATE(), @Activity, @Status)";
                    using (SqlCommand auditCmd = new SqlCommand(auditQuery, connection))
                    {
                        auditCmd.Parameters.AddWithValue("@UserID", userid);
                        auditCmd.Parameters.AddWithValue("@Activity", activity);
                        auditCmd.Parameters.AddWithValue("@Status", status);

                        // Execute the audit query
                        auditCmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                ScriptManager.RegisterStartupScript(this, GetType(), "auditError", $"alert('Audit log error: {ex.Message}');", true);
            }
        }


        private string GetUserRole(string username, string password)
        {
            string userRole = string.Empty;

            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();

                string query = "SELECT Role FROM accounts WHERE Username = @Username AND Password = @Password";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password); // Note: Password comparison should be handled securely

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Assuming the role is a string column in your database
                            userRole = reader["UserRole"].ToString();
                        }
                    }
                }
            }

            return userRole;
        }



        protected void loginValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            string username = unametxtbox.Text;
            string password = pwordtxtbox.Text;

            // Add your validation logic here
            bool isValid = false;

            // Example validation logic: Check if both fields are not empty
            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {

                if (username == "valid_username" && password == "valid_password")
                {
                    isValid = true;

                }
            }
            args.IsValid = isValid;
        }

        protected void unametxtbox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}