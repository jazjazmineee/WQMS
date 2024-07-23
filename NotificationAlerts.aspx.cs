using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WaterGuard
{
    public partial class NotificationAlerts : BasePage
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

                string query = "SELECT * FROM events";

                // Check if a date range is specified
                if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                {
                    // Filter by date range
                    query += $" WHERE DateTime >= @startDate AND DateTime < DATEADD(DAY, 1, @endDate)";
                }
                else if (!string.IsNullOrEmpty(selectedDate))
                {
                    // Filter by selected date
                    query += $" WHERE CONVERT(DATE, DateTime) = @selectedDate";
                }

                query += " ORDER BY DateTime DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
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

                /// Convert startdate and enddate to DateTime objects
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
                    ViewState["EndDate"] = endDateObj.ToString("yyyy-MM-dd");
                    //ViewState["EndDate"] = endDateObj.AddDays(1).ToString("yyyy-MM-dd"); // Include the end date

                    // Clear filtered data ViewState
                    ViewState["FilteredData"] = null;
                    ViewState["SelectedDate"] = null;

                    // Rebind GridView with filtered dates and reset page index
                    BindGridView(startDateObj.ToString("yyyy-MM-dd"), endDateObj.ToString("yyyy-MM-dd"));
                    //BindGridView(startDateObj.ToString("yyyy-MM-dd"), endDateObj.AddDays(1).ToString("yyyy-MM-dd"));
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
        protected void ShowAllDates_Click(object sender, EventArgs e)
        {
            // Redirect to the same page
            Response.Redirect("NotificationAlerts.aspx");
        }
        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Assuming the last column index is 2 (0-based index for 3 columns)
                int lastColumnIndex = 2;

                // Check the value in the last column and set the color accordingly
                string status = e.Row.Cells[lastColumnIndex].Text;

                if (status.Contains("SENT"))
                {
                    e.Row.Cells[lastColumnIndex].ForeColor = System.Drawing.Color.Green;
                }
                else if (status.Contains("Failed"))
                {
                    e.Row.Cells[lastColumnIndex].ForeColor = System.Drawing.Color.Red;
                }
            }
        }
        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            // Set the new page index
            GridView1.PageIndex = e.NewPageIndex;

            // Check if filtering by start/end date is applied
            if (ViewState["StartDate"] != null && ViewState["EndDate"] != null)
            {
                // If filtering by start/end date, bind the GridView with the filtered dates
                BindGridView(ViewState["StartDate"] as string, ViewState["EndDate"] as string);
            }
            else
            {
                // Otherwise, bind the GridView without any filters
                BindGridView();
            }
        }
    }
}
