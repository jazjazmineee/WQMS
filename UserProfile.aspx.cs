using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace WaterGuard_2024
{
    public partial class UserProfile : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                nameTxtbox.Text = Session["Name"].ToString();
                usernameTxtbox.Text = Session["Username"].ToString();
                contactTxtbox.Text = Session["Contact"].ToString();

                //Delete Expired OTPs to update Username
                DeleteExpiredOTPs();
            }

        }

        private string GetConnectionString()
        {
            return "Server=tcp:waterguard.database.windows.net,1433;Initial Catalog=waterqualitymonitoring;Persist Security Info=False;User ID=waterguardserver;Password=K7BVJwaterguard$;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }

        protected void saveChangesBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();

                    // Start building the query
                    string query = "UPDATE Accounts SET ";

                    // Initialize a list to hold the parameters
                    List<SqlParameter> parameters = new List<SqlParameter>();

                    // Check if the Name field is not empty
                    if (!string.IsNullOrEmpty(udName.Text))
                    {
                        query += "Name = @Name, ";
                        parameters.Add(new SqlParameter("@Name", udName.Text));
                    }

                    // Check if the Username field is not empty
                    if (!string.IsNullOrEmpty(udUsername.Text))
                    {
                        query += "Username = @Username, ";
                        parameters.Add(new SqlParameter("@Username", udUsername.Text));
                    }

                    // Check if the Contact field is not empty
                    if (!string.IsNullOrEmpty(udContact.Text))
                    {
                        query += "Contact = @Contact, ";
                        parameters.Add(new SqlParameter("@Contact", udContact.Text));
                    }

                    // Remove the trailing comma and space
                    query = query.TrimEnd(',', ' ');

                    // Add the WHERE clause
                    string userID = Session["UserID"].ToString(); // Assuming UserID is used as a unique identifier
                    query += " WHERE UserID = @UserID";
                    parameters.Add(new SqlParameter("@UserID", userID));

                    // Execute the query only if any fields were updated
                    if (parameters.Count > 0)
                    {
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddRange(parameters.ToArray());

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                // Database update successful
                                ScriptManager.RegisterStartupScript(this, GetType(), "myalert", "alert('Account is Successfully Updated!'); window.location.replace('LoginPage.aspx');", true);

                                LogAudit(userID, "Profile Update", "Successful");
                            }
                            else
                            {
                                // No rows were updated
                                ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", "showErrorAlert('No rows were updated.');", true);
                            }
                        }
                    }
                    else
                    {
                        // No fields to update
                        ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", "showErrorAlert('No fields were provided for update.');", true);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle and log the exception
                ScriptManager.RegisterStartupScript(this, GetType(), "myalert", $"alert('An error occurred: {ex.Message}');", true);
            }
        }


        private void DeleteExpiredOTPs()
        {
            // Implement logic to delete expired OTPs from the database
            string deleteQuery = "DELETE FROM OTPs WHERE DateTime < DATEADD(MINUTE, -5, GETDATE())";
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
        protected void udPassBtn_Click(object sender, EventArgs e)
        {
            try
            {
                string enteredCurrentPassword = currPassword.Text;
                string enteredNewPassword = newPassword.Text;
                string confirmedPassword = confirmPassword.Text;

                if (string.IsNullOrEmpty(enteredCurrentPassword) || string.IsNullOrEmpty(enteredNewPassword) || string.IsNullOrEmpty(confirmedPassword))
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", "showErrorAlert('All fields are required.');", true);

                    LogAudit(Session["UserID"].ToString(), "Password Update", "Unsuccessful");
                    return;

                }

                if (enteredNewPassword != confirmedPassword)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", "showErrorAlert('New password and confirm password do not match.');", true);

                    LogAudit(Session["UserID"].ToString(), "Password Update", "Unsuccessful");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();

                    string userID = Session["UserID"].ToString();

                    string query = "SELECT Password FROM Accounts WHERE UserID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userID);
                        string currentPassword = (string)cmd.ExecuteScalar();

                        if (currentPassword == enteredCurrentPassword)
                        {
                            query = "UPDATE Accounts SET Password = @Password WHERE UserID = @UserID";
                            using (SqlCommand updateCmd = new SqlCommand(query, conn))
                            {
                                updateCmd.Parameters.AddWithValue("@Password", enteredNewPassword);
                                updateCmd.Parameters.AddWithValue("@UserID", userID);
                                int rowsAffected = updateCmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    string successMessage = "Password is Successfully Updated!";
                                    ScriptManager.RegisterStartupScript(this, GetType(), "myalert", $"alert('{successMessage}');window.location='LoginPage.aspx?error={HttpUtility.UrlEncode(successMessage)}';", true);

                                    // Add to Audit
                                    LogAudit(userID, "Password Update", "Successful");
                                }
                                else
                                {
                                    ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", "showErrorAlert('No rows were updated.');", true);
                                }
                            }
                        }
                        else
                        {
                            ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", "showErrorAlert('Current password is incorrect.');", true);
                            currPassword.Text = "";
                            newPassword.Text = "";
                            confirmPassword.Text = "";

                            LogAudit(userID, "Password Update", "Unsuccessful");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "myalert", $"alert('An error occurred: {ex.Message}');", true);
            }
        }


        private void LogAudit(string userid, string activity, string status)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                try
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
                catch (Exception ex)
                {
                    // Handle exceptions
                    ScriptManager.RegisterStartupScript(this, GetType(), "auditError", $"alert('Audit log error: {ex.Message}');", true);
                }
            }
        }

    }
}
