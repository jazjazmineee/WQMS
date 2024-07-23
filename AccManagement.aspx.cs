using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WaterGuard_2024
{
    public partial class AccManagement : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ValidationSettings.UnobtrusiveValidationMode = UnobtrusiveValidationMode.None;

            if (!IsPostBack)
            {
                BindGridView();

                //Delete Expired OTPs to update Username
                DeleteExpiredOTPs();
            }
        }

        private string GetConnectionString()
        {
            return "Server=tcp:waterguard.database.windows.net,1433;Initial Catalog=waterqualitymonitoring;Persist Security Info=False;User ID=waterguardserver;Password=K7BVJwaterguard$;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }

        protected bool NameExistsInDatabase(string name, SqlConnection connection)
        {
            string selectQuery = "SELECT COUNT(*) FROM accounts WHERE Name = @Name";
            int count;

            using (SqlCommand command = new SqlCommand(selectQuery, connection))
            {
                command.Parameters.AddWithValue("@Name", name);
                count = Convert.ToInt32(command.ExecuteScalar());
            }

            // If count is greater than 0, the name exists in the database
            return count > 0;
        }

        private void DeleteExpiredOTPs()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                {
                    string deleteQuery = "DELETE FROM OTPs WHERE DateTime < @CurrentTime";
                    DateTime currentTime = DateTime.Now.AddMinutes(-5); // Subtract 5 minutes from the current time

                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@CurrentTime", currentTime);
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        Debug.WriteLine($"{rowsAffected} expired OTPs deleted successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle and log the exception
                ScriptManager.RegisterStartupScript(this, GetType(), "myalert", $"alert('An error occurred: {ex.Message}');", true);
            }
        }



        protected void SearchButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    con.Open();

                    // Check if the name exists in the database before executing the query
                    using (SqlCommand checkNameCommand = new SqlCommand("SELECT COUNT(*) FROM accounts WHERE Name = @Name", con))
                    {
                        checkNameCommand.Parameters.AddWithValue("@Name", udName.Text);
                        int nameCount = Convert.ToInt32(checkNameCommand.ExecuteScalar());

                        if (nameCount == 0)
                        {
                            // Name not found in the database, display an error message
                            ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", "showErrorAlert('Name not found in the database.');", true);

                            // Insert audit report
                            LogAudit(Session["UserID"].ToString(), "Search Account", "Unsuccessful");

                            // Clear filter condition
                            ViewState["FilteredData"] = null;

                            return;
                        }
                    }

                    using (SqlCommand search = new SqlCommand("SELECT * FROM accounts WHERE Name = @Name", con))
                    {
                        search.Parameters.AddWithValue("@Name", udName.Text);

                        using (SqlDataReader sitereader = search.ExecuteReader())
                        {
                            if (sitereader.HasRows)
                            {
                                sitereader.Read();
                                udUserName.Text = sitereader["Username"].ToString();
                                udUserRole.SelectedValue = sitereader["UserRole"].ToString();
                                udStatus.SelectedValue = sitereader["Status"].ToString();

                                // Show the updateUserModal using JavaScript function
                                ClientScript.RegisterStartupScript(this.GetType(), "showUpdateUserModal", "showUpdateUserModal();", true);

                                // Insert audit report
                                //LogAudit(Session["UserID"].ToString(), "Search Account", "Successful");

                                // Clear filter condition
                                ViewState["FilteredData"] = null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle and log the exception
                ScriptManager.RegisterStartupScript(this, GetType(), "myalert", $"alert('An error occurred: {ex.Message}');", true);
            }
        }


        protected void UdButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if any of the fields are empty
                if (string.IsNullOrWhiteSpace(udName.Text) ||
                    string.IsNullOrWhiteSpace(udUserName.Text) ||
                    string.IsNullOrWhiteSpace(udUserRole.SelectedValue) ||
                    string.IsNullOrWhiteSpace(udStatus.SelectedValue))
                {
                    // Display an error message if any field is empty
                    ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", "validationUpdate('All fields are required.');", true);
                    return; // Exit the method
                }

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();

                    // Format the selected date to match the database format
                    string inputName = udName.Text;
                    string inputUname = udUserName.Text;
                    string selectedRole = udUserRole.SelectedValue;
                    string selectedStatus = udStatus.SelectedValue;

                    // Check if the name exists in the database before proceeding with the update
                    if (NameExistsInDatabase(udName.Text, conn))
                    {
                        string query = "UPDATE accounts SET Username = @Username, UserRole = @UserRole, Status = @Status WHERE Name = @Name";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Name", inputName);
                            cmd.Parameters.AddWithValue("@Username", inputUname);
                            cmd.Parameters.AddWithValue("@UserRole", selectedRole);
                            cmd.Parameters.AddWithValue("@Status", selectedStatus);

                            // Execute the UPDATE query
                            cmd.ExecuteNonQuery();

                            // Clear filter condition
                            ViewState["FilteredData"] = null;

                            // Assuming the update was successful, bind the GridView
                            BindGridView();

                            // Display a success alert
                            ScriptManager.RegisterStartupScript(this, GetType(), "showSuccessAlert", "showSuccessAlert('Account is successfully updated!');", true);

                            // Insert audit report
                            LogAudit(Session["UserID"].ToString(), "Account Updated", "Successful");

                            // Clear the input fields
                            udName.Text = "";
                            udUserName.Text = "";
                            udUserRole.SelectedValue = "";
                            udStatus.SelectedValue = "";
                        }
                    }
                    else
                    {

                        // Display a message if the name does not exist in the database
                        ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", "showErrorAlert('Name does not exists in the database.');", true);

                        // Clear filter condition
                        ViewState["FilteredData"] = null;

                        // Insert audit report
                        LogAudit(Session["UserID"].ToString(), "Update Account", "Unsuccessful");

                        // Clear the input fields
                        udName.Text = "";
                        udUserName.Text = "";
                        udUserRole.SelectedValue = "";
                        udStatus.SelectedValue = "";
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle and log the exception
                ScriptManager.RegisterStartupScript(this, GetType(), "myalert", $"alert('An error occurred: {ex.Message}');", true);
            }
        }


        protected void AddButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if any of the required fields are empty
                if (string.IsNullOrWhiteSpace(name.Text) || string.IsNullOrWhiteSpace(userName.Text) ||
                    string.IsNullOrWhiteSpace(contact.Text) || string.IsNullOrWhiteSpace(userRole.SelectedValue) ||
                    string.IsNullOrWhiteSpace(status.SelectedValue))
                {
                    // Show an alert message if any required field is empty
                    //!!!!!!!!CHANGE
                    ScriptManager.RegisterStartupScript(this, GetType(), "myalert", "alert('All fields are required.');", true);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();

                    // Format the selected date to match the database format
                    string inputName = name.Text;
                    string inputUsername = userName.Text;
                    string inputContact = contact.Text;
                    string inputRole = userRole.SelectedValue;
                    string selectedStatus = status.SelectedValue;

                    // Check if the name already exists in the database before proceeding with the insert
                    if (!NameExistsInDatabase(name.Text, conn))
                    {
                        string query = "INSERT INTO accounts (Name, Username, Password, UserRole, Contact, Status) VALUES (@Name, @Username, 'Admin1234', @UserRole, @Contact, @Status)";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Name", inputName);
                            cmd.Parameters.AddWithValue("@Username", inputUsername);
                            cmd.Parameters.AddWithValue("@Contact", inputContact);
                            cmd.Parameters.AddWithValue("@UserRole", inputRole);
                            cmd.Parameters.AddWithValue("@Status", selectedStatus);

                            // Execute the INSERT query
                            cmd.ExecuteNonQuery();

                            // Clear filter condition
                            ViewState["FilteredData"] = null;

                            // Assuming GridView1 is the ID of your GridView, rebind it to update the displayed data
                            BindGridView();

                            // Display a success alert
                            ScriptManager.RegisterStartupScript(this, GetType(), "showSuccessAlert", "showSuccessAlert('Account is successfully added!');", true);



                            // Insert audit report
                            LogAudit(Session["UserID"].ToString(), "Account Added", "Successful");

                            // Clear the input fields
                            name.Text = "";
                            userName.Text = "";
                            contact.Text = "";
                            userRole.SelectedValue = "";
                            status.SelectedValue = "";
                        }
                    }
                    else
                    {
                        // Display an error message if the name already exists in the database
                        ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", "showErrorAlert('Name already exists in the database.');", true);

                        // Clear filter condition
                        ViewState["FilteredData"] = null;

                        // Insert audit report
                        LogAudit(Session["UserID"].ToString(), "Add Account", "Unsuccessful");

                        // Clear the input fields
                        name.Text = "";
                        userName.Text = "";
                        contact.Text = "";
                        userRole.SelectedValue = "";
                        status.SelectedValue = "";
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle and log the exception
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


        protected void AccountFilterBtn_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string selectedRowStatus = DropDownList1.SelectedValue;

                // If a row status is selected
                if (!string.IsNullOrEmpty(selectedRowStatus))
                {
                    string query = "SELECT UserID, Name, Username, Password, Contact, UserRole, Status FROM accounts";

                    // Add a filter for the selected row status
                    query += $" WHERE Status = @status";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@status", selectedRowStatus);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataSet dataSet = new DataSet();
                            adapter.Fill(dataSet);

                            // Store the filtered data in a DataTable
                            DataTable filteredData = dataSet.Tables[0];

                            // Save the filtered data in ViewState
                            ViewState["FilteredData"] = filteredData;

                            // Set the page index to the first page
                            GridView1.PageIndex = 0;

                            // Bind the GridView with the filtered data
                            GridView1.DataSource = filteredData;
                            GridView1.DataBind();
                        }
                    }
                }
                else // If no row status is selected
                {
                    // Fetch all data without any filtering
                    string query = "SELECT UserID, Name, Username, Password, Contact, UserRole, Status FROM accounts";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataSet dataSet = new DataSet();
                            adapter.Fill(dataSet);

                            // Store the filtered data in a DataTable
                            DataTable filteredData = dataSet.Tables[0];

                            // Save the filtered data in ViewState
                            ViewState["FilteredData"] = filteredData;

                            // Set the page index to the first page
                            GridView1.PageIndex = 0;

                            // Bind the GridView with the filtered data
                            GridView1.DataSource = filteredData;
                            GridView1.DataBind();
                        }
                    }
                }
            }
        }


        protected void ShowAllAccount_Click(object sender, EventArgs e)
        {
            // Clear filtered data ViewState
            ViewState["FilteredData"] = null;

            // Call BindGridView
            BindGridView();
        }

        protected void BindGridView()
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                //string query = "SELECT * FROM accounts";
                string query = "SELECT UserID, Name, Username, Contact, UserRole, Status FROM accounts";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet dataSet = new DataSet();
                        adapter.Fill(dataSet);

                        GridView1.DataSource = dataSet.Tables[0];
                        GridView1.DataBind();
                    }
                }
            }
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Assuming the last column index is 4 (0-based index for 5 columns)
                int lastColumnIndex = 5;

                // Check the value in the last column and set the color accordingly
                string status = e.Row.Cells[lastColumnIndex].Text;

                if (status.ToLower() == "inactive")
                {
                    e.Row.Cells[lastColumnIndex].ForeColor = System.Drawing.Color.Red;
                }
                else if (status.ToLower() == "active")
                {
                    e.Row.Cells[lastColumnIndex].ForeColor = System.Drawing.Color.Green;
                }
            }
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            // Set the new page index
            GridView1.PageIndex = e.NewPageIndex;


            if (ViewState["FilteredData"] != null)
            {
                // If filtering by status type, bind the GridView with the filtered data
                GridView1.DataSource = (DataTable)ViewState["FilteredData"];
                GridView1.DataBind();
            }
            else
            {

                // Otherwise, bind the GridView without any filters
                BindGridView();
            }
        }
    }
}
