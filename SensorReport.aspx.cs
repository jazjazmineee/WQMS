using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using System.Linq;
using MySql.Data.MySqlClient;
using PdfSharpCore.Drawing.Layout;
using System.Globalization;
using System.Collections.Generic;
using ClosedXML.Excel;



namespace WaterGuard_2024
{
    public partial class SensorReport : BasePage
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
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string query = "SELECT MeasurementDateTime, TDS, TDSStatus, pH, pHStatus, Turbidity, TurbidityStatus FROM SensorQualityMeasurement";

                // Check if a date range is specified
                if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                {
                    // Filter by date range
                    query += " WHERE " + GetDateFilterClause("MeasurementDateTime", startDate, endDate);
                }
                else if (!string.IsNullOrEmpty(startDate))
                {
                    // Filter by single date
                    query += " WHERE " + GetDateFilterClause("MeasurementDateTime", startDate);
                }
                else if (!string.IsNullOrEmpty(endDate))
                {
                    // Filter by end date
                    query += " WHERE " + GetDateFilterClause("MeasurementDateTime", null, endDate);
                }

                query += " ORDER BY MeasurementID DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Add parameters for date filtering
                    if (!string.IsNullOrEmpty(startDate))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                    }
                    if (!string.IsNullOrEmpty(endDate))
                    {
                        cmd.Parameters.AddWithValue("@endDate", endDate);
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet dataSet = new DataSet();
                        adapter.Fill(dataSet);

                        if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
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
        private string GetDateFilterClause(string columnName, string startDate = null, string endDate = null)
        {
            List<string> dateFilters = new List<string>();

            // Add filters for each supported date format
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 103) BETWEEN @startDate AND @endDate"); // dd/MM/yyyy
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 101) BETWEEN @startDate AND @endDate"); // MM/dd/yyyy
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 120) BETWEEN @startDate AND @endDate"); // yyyy-MM-dd

                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 121) BETWEEN @startDate AND @endDate"); // yyyy-MM-dd HH:mm:ss.mmm
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 126) BETWEEN @startDate AND @endDate"); // ISO 8601
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 112) BETWEEN @startDate AND @endDate"); // yyyymmdd

                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 106) BETWEEN @startDate AND @endDate"); // dd MMM yyyy
            }
            else if (!string.IsNullOrEmpty(startDate))
            {
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 103) = @startDate"); // dd/MM/yyyy
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 101) = @startDate"); // MM/dd/yyyy
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 120) = @startDate"); // yyyy-MM-dd

                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 121) = @startDate"); // yyyy-MM-dd HH:mm:ss.mmm
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 126) = @startDate"); // ISO 8601
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 112) = @startDate"); // yyyymmdd

                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 106) = @startDate"); // dd MMM yyyy
            }
            else if (!string.IsNullOrEmpty(endDate))
            {
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 103) <= @endDate");
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 101) <= @endDate");
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 120) <= @endDate");

                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 121) <= @endDate");
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 126) <= @endDate");
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 112) <= @endDate");

                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 106) <= @endDate");
            }

            // Combine filters with OR for flexibility
            return string.Join(" OR ", dateFilters);
        }
        protected void SingleSearch_Click(object sender, EventArgs e)
        {
            // Get the value from the single date textbox
            string singleDate = singleDateRange.Text.Trim();

            // Inside SingleSearch_Click method
            DateTime selectedDate;
            if (DateTime.TryParseExact(singleDate, new[] { "dd/MM/yyyy", "dd-MM-yyyy", "MM-dd-yyyy", "MM/dd/yyyy" }, CultureInfo.CurrentCulture, DateTimeStyles.None, out selectedDate))
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
            }
            else
            {
                string errorMessage = "Please select a valid date in one of the following formats: dd/MM/yyyy, dd-MM-yyyy, MM-dd-yyyy, MM/dd/yyyy";
                ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", $"showAlertAndHide('{errorMessage}');", true);
            }
        }
        protected void Search_Click(object sender, EventArgs e)
        {
            // Get the value from the daterange textbox
            string dateRange = daterange.Text.Trim();

            // Split the date range into start and end dates
            string[] dates = dateRange.Split('-');
            if (dates.Length == 2)
            {
                // Trim any whitespace from the start and end dates
                string startDate = dates[0].Trim();
                string endDate = dates[1].Trim();

                // Convert startdate and enddate to DateTime objects
                DateTime startDateObj, endDateObj;
                if (DateTime.TryParseExact(startDate, new[] { "dd/MM/yyyy", "dd-MM-yyyy", "MM-dd-yyyy", "MM/dd/yyyy" }, CultureInfo.CurrentCulture, DateTimeStyles.None, out startDateObj) &&
                    DateTime.TryParseExact(endDate, new[] { "dd/MM/yyyy", "dd-MM-yyyy", "MM-dd-yyyy", "MM/dd/yyyy" }, CultureInfo.CurrentCulture, DateTimeStyles.None, out endDateObj))
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

                    // Clear filtered data ViewState
                    ViewState["FilteredData"] = null;
                    ViewState["SelectedDate"] = null;

                    // Rebind GridView with filtered dates and reset page index
                    BindGridView(startDateObj.ToString("yyyy-MM-dd"), endDateObj.ToString("yyyy-MM-dd"));
                    GridView1.PageIndex = 0;
                }
                else
                {
                    // Display an alert message for invalid date formats
                    string errorMessage = "Please select a valid date range in one of the following formats: dd/MM/yyyy, dd-MM-yyyy, MM-dd-yyyy, MM/dd/yyyy";
                    ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", $"showAlertAndHide('{errorMessage}');", true);
                }
            }
            else
            {
                // Display an alert message for empty date range
                string errorMessage = "Please enter a valid date range in the format dd/MM/yyyy - dd/MM/yyyy";
                ScriptManager.RegisterStartupScript(this, GetType(), "showAlert", $"showAlertAndHide('{errorMessage}');", true);
            }
        }



        protected void filterBtn_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string selectedColumnType = DropDownList1.SelectedValue;
                string selectedRowStatus = DropDownList2.SelectedValue;

                if (!string.IsNullOrEmpty(selectedColumnType) && selectedColumnType != "Select Sensor Type")
                {
                    string statusColumnName = $"{selectedColumnType}Status";

                    string query = $@"SELECT MeasurementDateTime, 
                             {selectedColumnType}, {statusColumnName}
                      FROM SensorQualityMeasurement";

                    if (!string.IsNullOrEmpty(selectedRowStatus))
                    {
                        query += $" WHERE {statusColumnName} = @status";
                    }

                    query += " ORDER BY MeasurementID DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(selectedRowStatus))
                        {
                            cmd.Parameters.AddWithValue("@status", selectedRowStatus);
                        }

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataSet dataSet = new DataSet();
                            adapter.Fill(dataSet);

                            DataTable filteredData = dataSet.Tables[0];
                            ViewState["FilteredData"] = filteredData;
                            ViewState["StartDate"] = null;
                            ViewState["EndDate"] = null;
                            GridView1.PageIndex = 0;

                            // Clear existing columns
                            GridView1.Columns.Clear();

                            // Add dynamic columns based on selected sensor type
                            BoundField dateField = new BoundField();
                            dateField.DataField = "MeasurementDateTime";
                            dateField.HeaderText = "Date & Time";
                            GridView1.Columns.Add(dateField);

                            BoundField valueField = new BoundField();
                            valueField.DataField = selectedColumnType;
                            valueField.HeaderText = selectedColumnType;
                            GridView1.Columns.Add(valueField);

                            BoundField statusField = new BoundField();
                            statusField.DataField = statusColumnName;
                            statusField.HeaderText = $"{selectedColumnType} Status";
                            GridView1.Columns.Add(statusField);

                            GridView1.DataSource = filteredData;
                            GridView1.DataBind();
                        }
                    }
                }
                else // If "Show All" is selected or nothing is selected
                {
                    string query = @"SELECT MeasurementDateTime, 
                             CASE WHEN TDSStatus = @status THEN UPPER(TDSStatus) ELSE '' END AS TDSStatus,
                             CASE WHEN pHStatus = @status THEN UPPER(pHStatus) ELSE '' END AS pHStatus,
                             CASE WHEN TurbidityStatus = @status THEN UPPER(TurbidityStatus) ELSE '' END AS TurbidityStatus,
                             TDS,
                             pH,
                             Turbidity
                        FROM SensorQualityMeasurement";

                    // If a row status is selected, add a filter
                    if (!string.IsNullOrEmpty(selectedRowStatus))
                    {
                        query += " WHERE TDSStatus = @status OR pHStatus = @status OR TurbidityStatus = @status";
                    }

                    query += " ORDER BY MeasurementID DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(selectedRowStatus))
                        {
                            cmd.Parameters.AddWithValue("@status", selectedRowStatus);
                        }

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
            Response.Redirect("SensorReport.aspx");
        }
        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Iterate through each cell in the row
                foreach (TableCell cell in e.Row.Cells)
                {
                    // Check if the cell text represents a status column
                    SetStatusColor(cell);
                }
            }
        }
        // Method to set status color based on its value
        private void SetStatusColor(TableCell statusCell)
        {
            string status = statusCell.Text.Trim().ToLower(); // Trim and convert to lowercase to remove whitespace and ensure case-insensitivity

            switch (status)
            {
                case "normal":
                    statusCell.ForeColor = System.Drawing.Color.Green;
                    break;
                case "high":
                case "low":
                    statusCell.ForeColor = System.Drawing.Color.Red;
                    break;
                default:
                    statusCell.ForeColor = System.Drawing.Color.Black; // Default color for unexpected values
                    break;
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
            else if (ViewState["SelectedDate"] != null)
            {
                // If filtering by a single date, bind the GridView with the selected date
                BindGridView(ViewState["SelectedDate"] as string);
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
        protected void ExportBtn_Click(object sender, EventArgs e)
        {
            // Check if filtering by start/end date is applied
            if (ViewState["StartDate"] != null && ViewState["EndDate"] != null)
            {
                // Export filtered data by date range
                ExportToExcel(ViewState["StartDate"] as string, ViewState["EndDate"] as string);
            }
            else if (ViewState["SelectedDate"] != null)
            {
                // Export filtered data by a single date
                ExportToExcel(ViewState["SelectedDate"] as string);
            }
            else if (ViewState["FilteredData"] != null)
            {
                // Export filtered data by sensor type
                DataTable filteredData = ViewState["FilteredData"] as DataTable;
                ExportToExcel(filteredData);
            }
            else
            {
                // Export all data
                ExportToExcel();
            }
        }
        private void ExportToExcel(string startDate = null, string endDate = null)
        {
            DataTable dataTable;

            // Get data based on filters
            if (!string.IsNullOrEmpty(startDate) || !string.IsNullOrEmpty(endDate))
            {
                // Get filtered data by date range
                dataTable = GetFilteredData(startDate, endDate);
            }
            else
            {
                // Get all data
                dataTable = GetAllData();
            }

            // Export data to Excel
            using (XLWorkbook workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add(dataTable, "WaterQualityReport");

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=WaterQualityReport.xlsx");

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        private void ExportToExcel(DataTable dataTable)
        {
            using (XLWorkbook workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add(dataTable, "WaterQualityReport");

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=WaterQualityReport.xlsx");

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        private DataTable GetFilteredData(string startDate, string endDate)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                string query = "SELECT MeasurementDateTime, TDS, TDSStatus, pH, pHStatus, Turbidity, TurbidityStatus FROM SensorQualityMeasurement";

                if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                {
                    query += " WHERE " + GetDateFilterClause("MeasurementDateTime", startDate, endDate);
                }
                else if (!string.IsNullOrEmpty(startDate))
                {
                    query += " WHERE " + GetDateFilterClause("MeasurementDateTime", startDate);
                }
                else if (!string.IsNullOrEmpty(endDate))
                {
                    query += " WHERE " + GetDateFilterClause("MeasurementDateTime", null, endDate);
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(startDate))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                    }
                    if (!string.IsNullOrEmpty(endDate))
                    {
                        cmd.Parameters.AddWithValue("@endDate", endDate);
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        private DataTable GetAllData()
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                string query = "SELECT MeasurementDateTime, TDS, TDSStatus, pH, pHStatus, Turbidity, TurbidityStatus FROM SensorQualityMeasurement ORDER BY MeasurementID DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        protected void DownloadPDFLinkButton_Click(object sender, EventArgs e)
        {
            // Generate PDF and download
            GenerateAndDownloadPDF();
        }

        protected void GenerateAndDownloadPDF()
        {
            // Create a new PDF document
            PdfDocument document = new PdfDocument();

            // Add a page to the document
            PdfPage page = document.AddPage();

            // Get an XGraphics object for drawing on the page
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Fonts
            XFont titleFont = new XFont("Arial", 12, XFontStyle.Bold);
            XFont headerFont = new XFont("Arial", 10, XFontStyle.Bold);
            XFont dataFont = new XFont("Arial", 10); // Reduced font size for data cells
            XFont footerFont = new XFont("Arial", 8, XFontStyle.Regular); // Footer font

            // Logo
            XImage logo = XImage.FromFile(Server.MapPath("~/Images/CrystalClearHD.png"));
            double logoWidth = 300; // Set the desired width of the logo
            double logoHeight = (logo.PixelHeight * logoWidth) / logo.PixelWidth; // Maintain aspect ratio
            double logoX = (page.Width.Point - logoWidth) / 2; // Calculate the x position to center the logo at the top
            double logoY = 20; // Set the y position
            gfx.DrawImage(logo, logoX, logoY, logoWidth, logoHeight);

            // Draw horizontal line after logo
            double lineY = logoY + logoHeight + 10; // Set the y position for the line
            gfx.DrawLine(XPens.Black, 30, lineY, page.Width.Point - 30, lineY); // Adjust the line width as needed

            // Draw Sensor Report title
            double reportTitleY = logoY + logoHeight + 30; // Set the y position for the title
            gfx.DrawString("WATER QUALITY REPORT", titleFont, XBrushes.Black, new XRect(0, reportTitleY, page.Width, page.Height), XStringFormats.TopCenter);

            // Draw client name
            double clientNameY = reportTitleY + 30; // Set the y position for the client name
            string clientNamePrefix = "Franchisee: ";
            string clientNameValue = "Roman De Gala";

            gfx.DrawString(clientNamePrefix, headerFont, XBrushes.Black, new XRect(30, clientNameY, page.Width - 60, page.Height), XStringFormats.TopLeft);
            double clientNamePrefixWidth = gfx.MeasureString(clientNamePrefix, headerFont).Width;
            gfx.DrawString(clientNameValue, dataFont, XBrushes.Black, new XRect(30 + clientNamePrefixWidth, clientNameY, page.Width - 60 - clientNamePrefixWidth, page.Height), XStringFormats.TopLeft);

            // Draw sample description
            double descriptionY = clientNameY + 20; // Set the y position for the sample description
            string descriptionPrefix = "Sample Description: ";
            string descriptionValue = "Purified Drinking Water";

            gfx.DrawString(descriptionPrefix, headerFont, XBrushes.Black, new XRect(30, descriptionY, page.Width - 60, page.Height), XStringFormats.TopLeft);
            double descriptionPrefixWidth = gfx.MeasureString(descriptionPrefix, headerFont).Width;
            gfx.DrawString(descriptionValue, dataFont, XBrushes.Black, new XRect(30 + descriptionPrefixWidth, descriptionY, page.Width - 60 - descriptionPrefixWidth, page.Height), XStringFormats.TopLeft);

            // Draw refilling station name
            double stationNameY = descriptionY + 20; // Set the y position for the station name
            string stationNamePrefix = "Refilling Station Name: ";
            string stationNameValue = "Crystal Clear";

            gfx.DrawString(stationNamePrefix, headerFont, XBrushes.Black, new XRect(30, stationNameY, page.Width - 60, page.Height), XStringFormats.TopLeft);
            double stationNamePrefixWidth = gfx.MeasureString(stationNamePrefix, headerFont).Width;
            gfx.DrawString(stationNameValue, dataFont, XBrushes.Black, new XRect(30 + stationNamePrefixWidth, stationNameY, page.Width - 60 - stationNamePrefixWidth, page.Height), XStringFormats.TopLeft);

            // Draw refilling station location
            double stationLocationY = stationNameY + 20; // Set the y position for the station location
            string stationLocationPrefix = "Refilling Station Location: ";
            string stationLocationValue = "Crystal Clear South Point, 64V7+4FP, Southpoint Subdivision, Cabuyao, Laguna";

            gfx.DrawString(stationLocationPrefix, headerFont, XBrushes.Black, new XRect(30, stationLocationY, page.Width - 60, page.Height), XStringFormats.TopLeft);
            double stationLocationPrefixWidth = gfx.MeasureString(stationLocationPrefix, headerFont).Width;
            gfx.DrawString(stationLocationValue, dataFont, XBrushes.Black, new XRect(30 + stationLocationPrefixWidth, stationLocationY, page.Width - 60 - stationLocationPrefixWidth, page.Height), XStringFormats.TopLeft);

            // Draw date of download
            double downloadDateY = stationLocationY + 20; // Set the y position for the download date
            string downloadDatePrefix = "Date of Download: ";
            string downloadDateValue = DateTime.Now.ToString("MMMM dd yyyy");

            gfx.DrawString(downloadDatePrefix, headerFont, XBrushes.Black, new XRect(30, downloadDateY, page.Width - 60, page.Height), XStringFormats.TopLeft);
            double downloadDatePrefixWidth = gfx.MeasureString(downloadDatePrefix, headerFont).Width;
            gfx.DrawString(downloadDateValue, dataFont, XBrushes.Black, new XRect(30 + downloadDatePrefixWidth, downloadDateY, page.Width - 60 - downloadDatePrefixWidth, page.Height), XStringFormats.TopLeft);

            // Draw time of download
            double downloadTimeY = downloadDateY + 20; // Set the y position for the download time
            string downloadTimePrefix = "Time of Download: ";
            string downloadTimeValue = DateTime.Now.ToString("hh:mm tt");

            gfx.DrawString(downloadTimePrefix, headerFont, XBrushes.Black, new XRect(30, downloadTimeY, page.Width - 60, page.Height), XStringFormats.TopLeft);
            double downloadTimePrefixWidth = gfx.MeasureString(downloadTimePrefix, headerFont).Width;
            gfx.DrawString(downloadTimeValue, dataFont, XBrushes.Black, new XRect(30 + downloadTimePrefixWidth, downloadTimeY, page.Width - 60 - downloadTimePrefixWidth, page.Height), XStringFormats.TopLeft);

            // Draw horizontal line after time of download
            double line2Y = downloadTimeY + 25; // Set the y position for the line
            gfx.DrawLine(XPens.Black, 30, line2Y, page.Width.Point - 30, line2Y); // Adjust the line width as needed

            // Custom headers and values
            string[] customHeaders = { "Parameters", "Unit", "Results", "PNSDW Standards", "Remarks" };
            string[] parameters = { "Total Dissolved Solids (TDS)", "pH", "Turbidity" };
            string[] units = { "mg/L", " - ", "NTU" };
            double[] results = { 0, 0, 0 }; // Placeholder for total averages
            string[] standards = { "10", "5 - 7", "5.0 NTU" };
            string[] remarks = new string[3];

            // Fetch data from the database based on the current date
            DataTable dataTable = new DataTable();
            DateTime currentDate = DateTime.Now.Date; // Get the current date
            string formattedCurrentDate = currentDate.ToString("dd/MM/yyyy"); // Format the current date

            //DateTime targetDate = new DateTime(2024, 3, 20); // Set the target date to March 20, 2024
            //string formattedCurrentDate = targetDate.ToString("dd/MM/yyyy");

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                string query = $"SELECT TDS, pH, Turbidity FROM SensorQualityMeasurement WHERE CONVERT(VARCHAR(10), MeasurementDateTime, 103) = '{formattedCurrentDate}'";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }

            // Check if data table is empty
            if (dataTable.Rows.Count > 0)
            {
                // Calculate total averages
                results[0] = dataTable.AsEnumerable().Select(r => Convert.ToDouble(r["TDS"])).Average();
                results[1] = dataTable.AsEnumerable().Select(r => Convert.ToDouble(r["pH"])).Average();
                results[2] = dataTable.AsEnumerable().Select(r => Convert.ToDouble(r["Turbidity"])).Average();

                // Calculate remarks
                for (int i = 0; i < parameters.Length; i++)
                {
                    double result = results[i];
                    string standard = standards[i];

                    if (parameters[i] == "Turbidity") // Check if the parameter is Turbidity
                    {
                        if (result <= 5.0) // If turbidity is less than or equal to 5.0 NTU
                        {
                            remarks[i] = "PASSED";
                        }
                        else
                        {
                            remarks[i] = "FAILED";
                        }
                    }
                    else if (parameters[i] == "Total Dissolved Solids (TDS)") // Check if the parameter is TDS
                    {
                        if (result <= 10) // If TDS is less than or equal to 10
                        {
                            remarks[i] = "PASSED";
                        }
                        else
                        {
                            remarks[i] = "FAILED";
                        }
                    }
                    else // For other parameters, use the IsWithinStandard method
                    {
                        if (IsWithinStandard(result, standard))
                        {
                            remarks[i] = "PASSED";
                        }
                        else
                        {
                            remarks[i] = "FAILED";
                        }
                    }
                }
            }
            else
            {
                // No data found in the DataTable
                for (int i = 0; i < parameters.Length; i++)
                {
                    results[i] = 0; // Set results to 0 or any other default value
                    remarks[i] = "No data found";
                }
            }

            // Widths for each column 128 (Unit), 160 (Results), 60 (PNSDW), 110 (Remarks)
            int[] columnWidths = { 150, 60, 60, 110, 100 };

            // Draw custom headers
            int x = 90;
            int y = (int)downloadTimeY + 50; // Adjust the starting y position as needed
            for (int i = 0; i < customHeaders.Length; i++)
            {
                // Use the defined columnWidths array to set the width for each column
                gfx.DrawString(customHeaders[i], headerFont, XBrushes.Black, new XRect(x, y, columnWidths[i], 20), XStringFormats.TopLeft);
                x += columnWidths[i]; // Move to the next column
            }

            // Draw custom values
            x = 90;
            y += 20;
            for (int i = 0; i < parameters.Length; i++)
            {
                gfx.DrawString(parameters[i], dataFont, XBrushes.Black, new XRect(x, y, columnWidths[0], 20), XStringFormats.TopLeft);
                gfx.DrawString(units[i], dataFont, XBrushes.Black, new XRect(x + columnWidths[0], y, columnWidths[1], 20), XStringFormats.TopLeft);
                gfx.DrawString(results[i].ToString("F2"), dataFont, XBrushes.Black, new XRect(x + columnWidths[0] + columnWidths[1], y, columnWidths[2], 20), XStringFormats.TopLeft);
                gfx.DrawString(standards[i], dataFont, XBrushes.Black, new XRect(x + columnWidths[0] + columnWidths[1] + columnWidths[2], y, columnWidths[3], 20), XStringFormats.TopLeft);
                gfx.DrawString(remarks[i], dataFont, XBrushes.Black, new XRect(x + columnWidths[0] + columnWidths[1] + columnWidths[2] + columnWidths[3], y, columnWidths[4], 20), XStringFormats.TopLeft);

                y += 20; // Move to the next row
            }

            // Draw horizontal line after table
            double line3Y = y + 20; // Set the y position for the line below the table
            gfx.DrawLine(XPens.Black, 30, line3Y, page.Width.Point - 30, line3Y); // Adjust the line width as needed

            // Remark Passed
            double remarksPassedY = line3Y + 25; // Set the y position for the remarks description
            string remarksPassed = "REMARKS: PASSED - Indicates the parameter meets the standard. ";
            double remarksPassedWidth = gfx.MeasureString(remarksPassed, dataFont).Width;
            double remarksPassedHeight = gfx.MeasureString(remarksPassed, dataFont).Height;
            XRect remarksPassedRect = new XRect(30, remarksPassedY, remarksPassedWidth, remarksPassedHeight);
            gfx.DrawString(remarksPassed, dataFont, XBrushes.Black, remarksPassedRect, XStringFormats.TopLeft);
            // Remark Failed
            double remarksFailedY = remarksPassedY + remarksPassedHeight; // Set the y position for the reference
            string remarksFailed = "                    FAILED - Indicates the parameter does not meet the standard.";
            double remarksFailedWidth = gfx.MeasureString(remarksFailed, dataFont).Width;
            double remarksFailedHeight = gfx.MeasureString(remarksFailed, dataFont).Height;
            XRect remarksFailedRect = new XRect(30, remarksFailedY, remarksFailedWidth, remarksFailedHeight);
            gfx.DrawString(remarksFailed, dataFont, XBrushes.Black, remarksFailedRect, XStringFormats.TopLeft);

            // Reference 1
            double referenceY = remarksFailedY + remarksFailedHeight + 30; // Set the y position for the reference
            string reference = "REFERENCE: Administrative Order No. 2017 0010 (Philippine National Standards for Drinking Water of 2017).";
            double referenceWidth = gfx.MeasureString(reference, dataFont).Width;
            double referenceHeight = gfx.MeasureString(reference, dataFont).Height;
            XRect referenceRect = new XRect(30, referenceY, referenceWidth, referenceHeight);
            gfx.DrawString(reference, dataFont, XBrushes.Black, referenceRect, XStringFormats.TopLeft);

            // Reference 2
            double reference2Y = referenceY + referenceHeight; // Set the y position for the reference
            string reference2 = "                        Mandatory Drinking-Water Quality Parameters and their Standard Values.";
            double referenceWidth2 = gfx.MeasureString(reference2, dataFont).Width;
            double referenceHeight2 = gfx.MeasureString(reference2, dataFont).Height;
            XRect referenceRect2 = new XRect(30, reference2Y, referenceWidth2, referenceHeight2);
            gfx.DrawString(reference2, dataFont, XBrushes.Black, referenceRect2, XStringFormats.TopLeft);

            // Add footer
            string footerText = "Design and Implementation of Selected Water Refilling Stations Water Quality Type via Sensors Applied to Water Quality Monitoring System (2023-2024)\n\n" +
                "                           Jazmine Buenaventura   |   Ma. Patricia Claire Escalada   |   Ma. Teresa Ysabelle Visco   |   Angel Valerie Yapsangco";

            XRect footerRect = new XRect(30, page.Height - 55, page.Width - 40, 50);
            XTextFormatter tf = new XTextFormatter(gfx);
            tf.DrawString(footerText, footerFont, XBrushes.Black, footerRect);

            // Save the document to a MemoryStream
            MemoryStream ms = new MemoryStream();
            document.Save(ms);
            ms.Position = 0;

            // Download the PDF file
            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=WaterQualityReport.pdf");
            Response.Buffer = true;
            ms.WriteTo(Response.OutputStream);
            Response.End();
        }
        // Helper method to check if a value is within the standard range
        private bool IsWithinStandard(double value, string standard)
        {
            if (standard.Contains("-")) // Check for a range
            {
                string[] range = standard.Split('-');
                double lowerBound = Convert.ToDouble(range[0]);
                double upperBound = Convert.ToDouble(range[1]);
                return (value >= lowerBound && value <= upperBound);
            }
            else // Check for an exact value
            {
                double stdValue;
                if (double.TryParse(standard, out stdValue))
                {
                    // Check if the difference between values is within a tolerance
                    double tolerance = 0.001; // adjust this tolerance as needed
                    return Math.Abs(value - stdValue) <= tolerance;
                }
                else
                {
                    // Handle invalid standard values here
                    return false;
                }
            }
        }
    }
}
