using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WaterGuard_2024
{
    public partial class AuditReport : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Bind the GridView only when the page is loaded for the first time
                BindGridView();
            }
        }

        private string GetConnectionString()
        {
            return "Server=tcp:waterguard.database.windows.net,1433;Initial Catalog=waterqualitymonitoring;Persist Security Info=False;User ID=waterguardserver;Password=K7BVJwaterguard$;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }

        private void BindGridView(string startDate = null, string endDate = null)
        {
            string selectedDate = ViewState["SelectedDate"] as string;

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string query = "SELECT UserID, DateTime, Activity, Status FROM AuditReport";

                // Check if a date range is specified
                if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                {
                    // Filter by date range
                    query += $" WHERE DateTime BETWEEN @startDate AND @endDate";
                }
                else if (!string.IsNullOrEmpty(selectedDate))
                {
                    // Filter by selected date
                    query += $" WHERE CONVERT(DATE, DateTime) = @selectedDate";
                }

                query += " ORDER BY DateTime DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Add parameters for date filtering
                    if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate);
                    }
                    else if (!string.IsNullOrEmpty(selectedDate))
                    {
                        cmd.Parameters.AddWithValue("@selectedDate", selectedDate);
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet dataSet = new DataSet();
                        adapter.Fill(dataSet);

                        if (dataSet.Tables[0].Rows.Count > 0)
                        {
                            // Data is available, bind the GridView
                            GridView1.DataSource = dataSet.Tables[0];
                            GridView1.DataBind();
                        }
                        else
                        {
                            // No data found
                            GridView1.DataSource = null;
                            GridView1.DataBind();
                        }
                    }
                }
            }
        }

        protected void SingleSearch_Click(object sender, EventArgs e)
        {
            // Get the value from the single date textbox
            string singleDate = singleDateRange.Text;

            // Convert the singleDate to a DateTime object
            if (DateTime.TryParseExact(singleDate, new[] { "dd/MM/yyyy", "dd-MM-yyyy", "MM-dd-yyyy", "MM/dd/yyyy" }, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime selectedDate))
            {
                // Store the selected date in ViewState
                ViewState["SelectedDate"] = selectedDate.ToString("yyyy-MM-dd");

                // Clear filtered data ViewState
                ViewState["FilteredData"] = null;
                ViewState["StartDate"] = null;
                ViewState["EndDate"] = null;

                // Rebind GridView with the selected date and reset page index
                BindGridView(selectedDate.ToString("yyyy-MM-dd"));
                GridView1.PageIndex = 0;
                return;
            }
            else
            {
                string errorMessage = "Please select a date";
                ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", $"showAlertAndHide('{errorMessage}');", true);
            }
        }

        protected void Search_Click(object sender, EventArgs e)
        {
            // Get the value from the daterange textbox
            string dateRange = daterange.Text;

            // Split the date range into start and end dates
            string[] dates = dateRange.Split('-');
            if (dates.Length == 2)
            {
                // Trim any whitespace from the start and end dates
                string startDate = dates[0].Trim();
                string endDate = dates[1].Trim();

                // Convert startdate and enddate to DateTime objects
                if (DateTime.TryParseExact(startDate, new[] { "dd/MM/yyyy", "dd-MM-yyyy", "MM-dd-yyyy", "MM/dd/yyyy" }, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime startDateObj) &&
                    DateTime.TryParseExact(endDate, new[] { "dd/MM/yyyy", "dd-MM-yyyy", "MM-dd-yyyy", "MM/dd/yyyy" }, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime endDateObj))
                {
                    // Check if start date is greater than end date
                    if (startDateObj > endDateObj)
                    {
                        string errorMessage = "End date must be greater than or equal to start date.";
                        ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", $"showAlertAndHide('{errorMessage}');", true);
                        return;
                    }

                    // Store filter values in ViewState
                    ViewState["StartDate"] = startDateObj.ToString("yyyy-MM-dd");
                    ViewState["EndDate"] = endDateObj.AddDays(1).ToString("yyyy-MM-dd"); // Include the end date

                    // Clear filtered data ViewState
                    ViewState["FilteredData"] = null;
                    ViewState["SelectedDate"] = null;

                    // Rebind GridView with filtered dates and reset page index
                    BindGridView(startDateObj.ToString("yyyy-MM-dd"), endDateObj.AddDays(1).ToString("yyyy-MM-dd"));
                    GridView1.PageIndex = 0;
                    return;
                }
            }
            else
            {
                // Display an alert message for empty date
                string errorMessage = "Please select date range";
                ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", $"showAlertAndHide('{errorMessage}');", true);
            }
        }

        protected void AuditFilterBtn_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string selectedRowStatus = DropDownList1.SelectedValue;

                // If a row status is selected
                if (!string.IsNullOrEmpty(selectedRowStatus))
                {
                    string query = "SELECT UserID, DateTime, Activity, Status FROM AuditReport";

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

                            // Clear start/end date ViewState
                            ViewState["StartDate"] = null;
                            ViewState["EndDate"] = null;
                            ViewState["SelectedDate"] = null;

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
                    string query = "SELECT UserID, DateTime, Activity, Status FROM AuditReport";

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

                            // Clear start/end date ViewState
                            ViewState["StartDate"] = null;
                            ViewState["EndDate"] = null;
                            ViewState["SelectedDate"] = null;

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

        protected void ShowAllDates_Click(object sender, EventArgs e)
        {
            // Redirect to the same page
            Response.Redirect("AuditReport.aspx");
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow) 
            {
                // Get the column indices based on column names
                int AuditStatus = -1;

                // Get the column index
                if (GridView1.DataSource != null && GridView1.DataSource is DataTable)
                {
                    DataTable dt = (DataTable)GridView1.DataSource;
                    if (dt.Columns.Contains("Status"))
                        AuditStatus = dt.Columns.IndexOf("Status");
                }

                // Check the value in the Audit Report Status column and set the color accordingly
                if (AuditStatus != -1)
                {
                    string Status = e.Row.Cells[AuditStatus].Text.Trim(); // Trim to remove whitespace
                    if (Status.Equals("successful", StringComparison.OrdinalIgnoreCase))
                    {
                        e.Row.Cells[AuditStatus].ForeColor = System.Drawing.Color.Green;
                    }
                    else if (Status.Equals("unsuccessful", StringComparison.OrdinalIgnoreCase))
                    {
                        e.Row.Cells[AuditStatus].ForeColor = System.Drawing.Color.Red;
                    }
                    else
                    {
                        // Log or display the value for debugging
                        System.Diagnostics.Debug.WriteLine("Unexpected value in Status column: " + Status);
                    }
                }
            }
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            // Set the new page index
            GridView1.PageIndex = e.NewPageIndex;

            // Check if filtering by start/end/selected date is applied
            if (ViewState["StartDate"] != null && ViewState["EndDate"] != null)
            {
                // If filtering by start/end date, bind the GridView with the filtered dates
                BindGridView(ViewState["StartDate"] as string, ViewState["EndDate"] as string);
            }
            else if (ViewState["FilteredData"] != null)
            {
                // If filtering by sensor type, bind the GridView with the filtered data
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
