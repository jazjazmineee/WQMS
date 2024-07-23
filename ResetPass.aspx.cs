using MySql.Data.MySqlClient;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Web.UI;
using System.Data;
using System.Data.SqlClient;

namespace WaterGuard_2024
{
    public partial class ResetPass : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ValidationSettings.UnobtrusiveValidationMode = UnobtrusiveValidationMode.None;
            }
        }

        private string GetConnectionString()
        {
            return "Server=tcp:waterguard.database.windows.net,1433;Initial Catalog=waterqualitymonitoring;Persist Security Info=False;User ID=waterguardserver;Password=K7BVJwaterguard$;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }

        protected void sendOTPBtn_Click(object sender, EventArgs e)
        {
            // Retrieve username, new password, and confirm password from the input fields
            string enteredUsername = uname.Text;
            string newPass = newPassword.Text;
            string confirmPass = confirmPassword.Text;

            bool userExists = CheckIfUsernameExists(enteredUsername);

            if (userExists)
            {
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                {
                    try
                    {
                        connection.Open();

                        string userActiveQuery = "SELECT Status, UserID FROM Accounts WHERE Username = @username";
                        using (SqlCommand userActiveCmd = new SqlCommand(userActiveQuery, connection))
                        {
                            userActiveCmd.Parameters.AddWithValue("@username", enteredUsername);
                            using (SqlDataReader reader = userActiveCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    string status = reader["Status"].ToString();
                                    string userid = reader["UserID"].ToString();

                                    if (string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if ((newPass == "" && confirmPass == "") || newPass != confirmPass)
                                        {
                                            // Passwords don't match or both are null
                                            error_alert.InnerText = "Please double check new password and confirm password."; // Change the text accordingly
                                            error_alert.Visible = true;
                                            ScriptManager.RegisterStartupScript(this, GetType(), "showErrorAlert", "showAlertAndHide('error_alert');", true);
                                            return; // Stop execution
                                        }
                                        else
                                        {
                                            // Clear any previous error messages
                                            error_alert.InnerText = "";
                                            error_alert.Visible = false;

                                            // User is active, generate and send OTP
                                            SendOTP(enteredUsername);

                                            // Disable the "Resend OTP" button until the timer expires
                                            sendOTPBtn.Enabled = false;

                                            ScriptManager.RegisterStartupScript(this, GetType(), "startCountdown", $"startCountdown(300);", true);

                                            // Start the countdown manually
                                            ViewState["StartTime"] = DateTime.Now;
                                            hdnRemainingTime.Value = "300"; // Set the initial countdown time to 5 minutes (300 seconds)
                                            ScriptManager.RegisterStartupScript(this, GetType(), "startCountdown", "startCountdown();", true);

                                            // Store UserID in session
                                            Session["UserID"] = userid;
                                        }
                                    }
                                    else
                                    {
                                        // User is not active, do not send OTP
                                        error_alert.InnerText = "User is not active. OTP not sent."; // Change the text accordingly
                                        error_alert.Visible = true;
                                        ScriptManager.RegisterStartupScript(this, GetType(), "showErrorAlert", "showAlertAndHide('error_alert');", true);
                                    }
                                }
                                else
                                {
                                    // Username not found
                                    error_alert.InnerText = "Invalid username.";
                                    error_alert.Visible = true;
                                    ScriptManager.RegisterStartupScript(this, GetType(), "showErrorAlert", "showAlertAndHide('error_alert');", true);
                                    return; // Stop execution
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions
                        ScriptManager.RegisterStartupScript(this, GetType(), "sendOTPError", $"alert('Error checking user status: {ex.Message}');", true);
                    }
                    finally
                    {
                        // Close the connection
                        if (connection.State == ConnectionState.Open)
                            connection.Close();
                    }
                }
            }
            else
            {
                // Username does not exist, show error message
                error_alert.InnerText = "Invalid username.";
                error_alert.Visible = true;
                ScriptManager.RegisterStartupScript(this, GetType(), "showErrorAlert", "showAlertAndHide('error_alert');", true);
                return; // Stop execution
            }
        }

        private bool CheckIfUsernameExists(string username)
        {
            bool exists = false;
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    connection.Open();

                    string checkUsernameQuery = "SELECT COUNT(*) FROM Accounts WHERE Username = @username";
                    using (SqlCommand checkUsernameCmd = new SqlCommand(checkUsernameQuery, connection))
                    {
                        checkUsernameCmd.Parameters.AddWithValue("@username", username);
                        int count = Convert.ToInt32(checkUsernameCmd.ExecuteScalar());
                        exists = (count > 0);
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                    ScriptManager.RegisterStartupScript(this, GetType(), "checkUsernameError", $"alert('Error checking username: {ex.Message}');", true);
                }
                finally
                {
                    // Close the connection
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                }
            }
            return exists;
        }

        protected bool ValidateOTP(SqlConnection connection, string username, string enteredOTP)
        {
            // Retrieve the stored OTP for the user from the database
            string otpCheckQuery = "SELECT OTP, DateTime FROM OTPs WHERE Username = @username ORDER BY DateTime DESC LIMIT 1";
            using (SqlCommand cmd = new SqlCommand(otpCheckQuery, connection))
            {
                cmd.Parameters.AddWithValue("@username", username);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    try
                    {
                        if (reader.Read())
                        {
                            string storedOTP = reader["OTP"].ToString();
                            DateTime creationTime = Convert.ToDateTime(reader["DateTime"]);

                            // Compare the entered OTP with the stored OTP and check the validity period
                            if (storedOTP == enteredOTP && (DateTime.Now - creationTime).TotalMinutes <= 300) // 5 minutes
                            {
                                return true; // OTP is valid
                            }

                        }
                    }
                    finally
                    {
                        // Ensure the DataReader is closed
                        reader.Close();
                    }
                }
            }
            return false; // OTP is invalid or expired
        }

        protected void SendOTP(string username)
        {
            // Disable the OTP button
            sendOTPBtn.Enabled = false;

            string apikey = "241c4f578f391bd521fba494c961a3bb"; // Replace with your actual Semaphore API key
            string messageTemplate = "Please use it within 5 minutes."; // Message template without {otp} placeholder

            // Retrieve the phone number associated with the username from the database
            string phoneNumber = GetPhoneNumberFromDatabase(username);

            if (phoneNumber == null)
            {
                // Phone number not found in the database for the given username
                error_alert.InnerText = "Contact not found of the given username."; // Change the text accordingly
                error_alert.Visible = true;
                ScriptManager.RegisterStartupScript(this, GetType(), "showErrorAlert", "showAlertAndHide('error_alert');", true);

                // Insert to events database with failure status and null phoneNumber
                LogSentMessage(username, false);

                // Re-enable the OTP button
                sendOTPBtn.Enabled = true;

                return;
            }

            string otp1 = GenerateOTP();

            // Store the OTP in the database
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    StoreOTPInDatabase(connection, username, otp1);
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                    ScriptManager.RegisterStartupScript(this, GetType(), "otpStoreError", $"alert('Error storing OTP: {ex.Message}');", true);

                    // Re-enable the OTP button
                    sendOTPBtn.Enabled = true;

                    return; // Stop execution if an error occurs while storing OTP
                }
            }

            // Send the OTP message using Semaphore API
            using (WebClient client = new WebClient())
            {
                try
                {
                    var values = new NameValueCollection()
                    {
                        { "apikey", apikey },
                        { "number", phoneNumber }, // Use the retrieved phone number
                        { "message", messageTemplate },
                        { "sendername", "WATERGUARD" }, // Replace with your sender name
                        { "code", otp1 } // Pass the generated OTP directly to the Semaphore API
                    };

                    byte[] response = client.UploadValues("https://api.semaphore.co/api/v4/otp", values);
                    string result = System.Text.Encoding.UTF8.GetString(response);

                    // OTP successfully sent
                    LogSentMessage(username, true, phoneNumber);

                    // Display a success message alert using JavaScript
                    success_alert.InnerText = "OTP sent successfully!"; // Change the text accordingly
                    success_alert.Visible = true;
                    ScriptManager.RegisterStartupScript(this, GetType(), "showSuccessAlert", "showAlertAndHide('success_alert');", true);


                    // Re-enable the OTP button after 5 minutes
                    string script = @"setTimeout(function() { document.getElementById('" + sendOTPBtn.ClientID + @"').disabled = false; }, 300000);"; // 300000 milliseconds = 5 minute
                    ScriptManager.RegisterStartupScript(this, GetType(), "reenableOTPButton", script, true);
                }
                catch (Exception)
                {
                    // Display an error message alert using JavaScript
                    error_alert.InnerText = "Error sending OTP. Please try again later."; // Change the text accordingly
                    error_alert.Visible = true;
                    ScriptManager.RegisterStartupScript(this, GetType(), "showErrorAlert", "showAlertAndHide('error_alert');", true);

                    // Re-enable the OTP button
                    sendOTPBtn.Enabled = true;
                }
            }
        }

        private void LogSentMessage(string username, bool success, string phoneNumber = null)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string message = success ? $"OTP SENT to {phoneNumber}" : "Failed to send OTP: Contact not found";

                string query = "INSERT INTO Events (DateTime, Message) VALUES (@DateTime, @Message)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("@Message", message);

                    // Execute the query
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected string GetPhoneNumberFromDatabase(string username)
        {
            string phoneNumber = null;

            // Query the database to retrieve the phone number associated with the username
            string query = "SELECT Contact FROM Accounts WHERE Username = @username";
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        phoneNumber = result.ToString();
                    }
                }
            }

            return phoneNumber;
        }

        protected string GenerateOTP()
        {
            // Generate a random OTP (e.g., a 6-digit number)
            Random rand = new Random();
            int otp = rand.Next(100000, 999999);
            return otp.ToString();
        }

        protected void StoreOTPInDatabase(SqlConnection connection, string username, string otp)
        {
            // Store the OTP in the database for verification during password reset process
            string insertQuery = "INSERT INTO OTPs (Username, OTP, DateTime) VALUES (@username, @otp, @datetime)";
            using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@otp", otp);
                cmd.Parameters.AddWithValue("@datetime", DateTime.Now); // Insert the current datetime
                cmd.ExecuteNonQuery();
            }
        }

        private void UpdatePassword(SqlConnection connection, string username, string newPassword)
        {
            try
            {
                string updateQuery = "UPDATE Accounts SET Password = @NewPassword WHERE Username = @Username";
                using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                {
                    updateCmd.Parameters.AddWithValue("@NewPassword", newPassword);
                    updateCmd.Parameters.AddWithValue("@Username", username);

                    // Execute the update query
                    updateCmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                ScriptManager.RegisterStartupScript(this, GetType(), "updatePasswordError", $"alert('Error updating password: {ex.Message}');", true);
            }
        }

        protected void resetPasswordSubmitBtn_Click(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    connection.Open();

                    // Retrieve username, new password, and OTP from the input fields
                    string enteredUsername = uname.Text;
                    string newPass = newPassword.Text;
                    string confirmPass = confirmPassword.Text;
                    string enteredOTP = otp.Text;

                    // Check if the username exists in the database
                    string userCheckQuery = "SELECT COUNT(*) FROM Accounts WHERE Username = @username";
                    using (SqlCommand userCheckCmd = new SqlCommand(userCheckQuery, connection))
                    {
                        userCheckCmd.Parameters.AddWithValue("@username", enteredUsername);
                        int userCount = Convert.ToInt32(userCheckCmd.ExecuteScalar());

                        if (userCount > 0)
                        {
                            // Check if the user is active
                            string userActiveQuery = "SELECT Status, UserID FROM Accounts WHERE Username = @username";
                            using (SqlCommand userActiveCmd = new SqlCommand(userActiveQuery, connection))
                            {
                                userActiveCmd.Parameters.AddWithValue("@username", enteredUsername);
                                using (SqlDataReader reader = userActiveCmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        string status = reader["Status"].ToString();
                                        string userid = reader["UserID"].ToString();

                                        // Close the reader after reading
                                        reader.Close();

                                        if (string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
                                        {
                                            // Check if the OTP exists in the database and if it is not expired
                                            string otpCheckQuery = "SELECT OTP, DateTime FROM OTPs WHERE Username = @username ORDER BY DateTime DESC";
                                            using (SqlCommand otpCheckCmd = new SqlCommand(otpCheckQuery, connection))
                                            {
                                                otpCheckCmd.Parameters.AddWithValue("@username", enteredUsername);
                                                using (SqlDataReader outerReader = otpCheckCmd.ExecuteReader())
                                                {
                                                    if (outerReader.Read())
                                                    {
                                                        string storedOTP = outerReader["OTP"].ToString();
                                                        DateTime creationTime = Convert.ToDateTime(outerReader["DateTime"]);

                                                        // Compare the entered OTP with the stored OTP and check the validity period
                                                        if (storedOTP == enteredOTP && (DateTime.Now - creationTime).TotalMinutes <= 5) // Valid for 5 minutes
                                                        {
                                                            // OTP is valid, proceed with password reset
                                                            if (newPass != confirmPass)
                                                            {
                                                                // Clear any previous success messages
                                                                success_alert.InnerText = "";
                                                                success_alert.Visible = false;

                                                                // Passwords don't match
                                                                error_alert.InnerText = "New password and confirm password do not match."; // Change the text accordingly
                                                                error_alert.Visible = true;
                                                                ScriptManager.RegisterStartupScript(this, GetType(), "showErrorAlert", "showAlertAndHide('error_alert');", true);

                                                                if (!string.IsNullOrEmpty(hdnRemainingTime.Value))
                                                                {
                                                                    // Continue the countdown timer
                                                                    int remainingSeconds = int.Parse(hdnRemainingTime.Value);
                                                                    ScriptManager.RegisterStartupScript(this, GetType(), "startCountdown", $"startCountdown({remainingSeconds});", true);
                                                                }

                                                                // Add to Audit
                                                                LogAudit(userid, "Confirm password for reset", "Unsuccessful");
                                                            }
                                                            else if (string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(confirmPass))
                                                            {
                                                                // Clear any previous success messages
                                                                success_alert.InnerText = "";
                                                                success_alert.Visible = false;

                                                                // New password or confirm password is empty
                                                                error_alert.InnerText = "New password and confirm password must not be empty."; // Change the text accordingly
                                                                error_alert.Visible = true;
                                                                ScriptManager.RegisterStartupScript(this, GetType(), "showErrorAlert", "showAlertAndHide('error_alert');", true);

                                                                if (!string.IsNullOrEmpty(hdnRemainingTime.Value))
                                                                {
                                                                    // Continue the countdown timer
                                                                    int remainingSeconds = int.Parse(hdnRemainingTime.Value);
                                                                    ScriptManager.RegisterStartupScript(this, GetType(), "startCountdown", $"startCountdown({remainingSeconds});", true);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                // Close the outer reader after reading
                                                                outerReader.Close();

                                                                // Clear any previous success messages
                                                                success_alert.InnerText = "";
                                                                success_alert.Visible = false;

                                                                // Passwords match, update the password in the database
                                                                UpdatePassword(connection, enteredUsername, confirmPass);

                                                                // Add to Audit
                                                                LogAudit(userid, "Reset password", "Successful");

                                                                // Provide feedback to the user
                                                                ScriptManager.RegisterStartupScript(this, GetType(), "passwordResetSuccess", "alert('Password reset successful.'); window.location.href = 'LoginPage.aspx';", true);
                                                            }
                                                        }
                                                        else if (storedOTP != enteredOTP && (DateTime.Now - creationTime).TotalMinutes <= 5)
                                                        {
                                                            // Clear any previous success messages
                                                            success_alert.InnerText = "";
                                                            success_alert.Visible = false;

                                                            error_alert.InnerText = "Invalid OTP. Please try again."; // Change the text accordingly
                                                            error_alert.Visible = true;
                                                            ScriptManager.RegisterStartupScript(this, GetType(), "showErrorAlert", "showAlertAndHide('error_alert');", true);

                                                            // Clear any previous countdown interval
                                                            //ScriptManager.RegisterStartupScript(this, GetType(), "clearCountdownInterval", "clearCountdownInterval();", true);

                                                            if (!string.IsNullOrEmpty(hdnRemainingTime.Value))
                                                            {
                                                                // Continue the countdown timer
                                                                int remainingSeconds = int.Parse(hdnRemainingTime.Value);
                                                                ScriptManager.RegisterStartupScript(this, GetType(), "startCountdown", $"startCountdown({remainingSeconds});", true);
                                                            }


                                                            // Add to Audit
                                                            LogAudit(userid, "Invalid OTP for reset password", "Unsuccessful");
                                                        }
                                                        else
                                                        {
                                                            // Clear any previous success messages
                                                            success_alert.InnerText = "";
                                                            success_alert.Visible = false;

                                                            // OTP is invalid or expired
                                                            error_alert.InnerText = "Expired or Invalid OTP. Please request a new OTP."; // Change the text accordingly
                                                            error_alert.Visible = true;
                                                            ScriptManager.RegisterStartupScript(this, GetType(), "showErrorAlert", "showAlertAndHide('error_alert');", true);

                                                            // Add to Audit
                                                            LogAudit(userid, "OTP validation for password reset", "Unsuccessful");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // Clear any previous success messages
                                                        success_alert.InnerText = "";
                                                        success_alert.Visible = false;

                                                        // OTP not found
                                                        error_alert.InnerText = "OTP not found. Please request a new OTP."; // Change the text accordingly
                                                        error_alert.Visible = true;
                                                        ScriptManager.RegisterStartupScript(this, GetType(), "showErrorAlert", "showAlertAndHide('error_alert');", true);

                                                        // Add to Audit
                                                        LogAudit(userid, "OTP not found for reset password", "Unsuccessful");
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Clear any previous success messages
                                            success_alert.InnerText = "";
                                            success_alert.Visible = false;

                                            // User is not active
                                            error_alert.InnerText = "User is not active. Please contact your administrator."; // Change the text accordingly
                                            error_alert.Visible = true;
                                            ScriptManager.RegisterStartupScript(this, GetType(), "showErrorAlert", "showAlertAndHide('error_alert');", true);

                                            // Add to Audit
                                            LogAudit(userid, "Reset password", "Unsuccessful");
                                        }
                                    }
                                    else
                                    {
                                        // Clear any previous success messages
                                        success_alert.InnerText = "";
                                        success_alert.Visible = false;

                                        // Username not found in the database
                                        error_alert.InnerText = "Invalid username."; // Change the text accordingly
                                        error_alert.Visible = true;
                                        ScriptManager.RegisterStartupScript(this, GetType(), "showErrorAlert", "showAlertAndHide('error_alert');", true);

                                        // Add to Audit
                                        LogAudit(enteredUsername, "User not registered", "Unsuccessful");
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Clear any previous success messages
                            success_alert.InnerText = "";
                            success_alert.Visible = false;

                            // Username not found in the database
                            error_alert.InnerText = "Invalid username."; // Change the text accordingly
                            error_alert.Visible = true;
                            ScriptManager.RegisterStartupScript(this, GetType(), "showErrorAlert", "showAlertAndHide('error_alert');", true);

                            // Add to Audit
                            LogAudit(enteredUsername, "User not registered", "Unsuccessful");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                    ScriptManager.RegisterStartupScript(this, GetType(), "resetPasswordError", $"alert('Error resetting password: {ex.Message}');", true);
                }
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

        protected void closeBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect("LoginPage.aspx", false);
        }
    }
}
