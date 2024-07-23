using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using System.Net;
using System.Drawing;
using System.Web.UI.DataVisualization.Charting;
using System.Globalization;
using System.Web.UI;

namespace WaterGuard_2024
{
    public partial class Dashboard : BasePage
    {
        private const string semaphoreApiKey = "241c4f578f391bd521fba494c961a3bb"; // Replace with your Semaphore API key
        private const string LastSmsSentKey = "LastSmsSent";
        private System.Timers.Timer qualityLoggingTimer;
        private TimeSpan morningReportTime = new TimeSpan(8, 0, 0); // 8 AM
        private TimeSpan eveningReportTime = new TimeSpan(17, 0, 0); // 5 PM
        private static readonly object reportLock = new object();

        private bool morningReportSent = false;
        private bool eveningReportSent = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Set the default filter to 'week' on initial load
                FilterDropDown.SelectedValue = "week";
                BindLineChart();
            }

            BindFormViewDataPH();
            BindFormViewDataTDS();
            BindFormViewDataTurbidity();

            DisplayTankCleaningMessage();

            BindGridView();

            BindTDSChart();
            BindpHChart();
            BindTurbidityChart();

            // Manually wire the event
            FormView1.PageIndexChanging += FormView1_PageIndexChanging;
            FormView2.PageIndexChanging += FormView2_PageIndexChanging;
            FormView3.PageIndexChanging += FormView3_PageIndexChanging;

            // Check levels and send SMS if high, but only if SMS has not been sent after last data change
            CheckAndSendNotifications();

            // Start the timer to check quality every 30 minutes
            if (!IsPostBack && Session["QualityLoggingTimerStarted"] == null)
            {
                StartQualityCheckingTimer();
                Session["QualityLoggingTimerStarted"] = true;
            }

            // Schedule daily reports
            ScheduleDailyReports();
        }
        private void DisplayTankCleaningMessage()
        {
            // Check TDS Status
            if (FormView5.DataItemCount > 0)
            {
                string tdsStatus = DataBinder.Eval(FormView5.DataItem, "TDSStatus").ToString();
                if (tdsStatus == "HIGH")
                {
                    LabelTDSMessage.Visible = true;
                }
            }

            // Check pH Status
            if (FormView4.DataItemCount > 0)
            {
                string pHStatus = DataBinder.Eval(FormView4.DataItem, "pHStatus").ToString();
                if (pHStatus == "HIGH" || pHStatus == "LOW")
                {
                    LabelPHMessage.Visible = true;
                }
            }

            // Check Turbidity Status
            if (FormView6.DataItemCount > 0)
            {
                string turbidityStatus = DataBinder.Eval(FormView6.DataItem, "TurbidityStatus").ToString();
                if (turbidityStatus == "HIGH")
                {
                    LabelTurbidityMessage.Visible = true;
                }
            }
        }
        private string GetConnectionString()
        {
            return "Server=tcp:waterguard.database.windows.net,1433;Initial Catalog=waterqualitymonitoring;Persist Security Info=False;User ID=waterguardserver;Password=K7BVJwaterguard$;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }

        protected void updateDashboard(object sender, EventArgs e)
        {
            FormView1.PageIndexChanging += FormView1_PageIndexChanging;
            FormView2.PageIndexChanging += FormView2_PageIndexChanging;
            FormView3.PageIndexChanging += FormView3_PageIndexChanging;
            BindGridView();
            BindTDSChart();
            BindpHChart();
            BindTurbidityChart();
            CheckAndSendNotifications();
        }

        private Dictionary<string, double> LastReadings
        {
            get
            {
                if (Session["LastReadings"] == null)
                {
                    Session["LastReadings"] = new Dictionary<string, double>
                {
                    { "pH", 0.0 },
                    { "TDS", 0.0 },
                    { "Turbidity", 0.0 }
                };
                }

                return (Dictionary<string, double>)Session["LastReadings"];
            }
            set
            {
                Session["LastReadings"] = value;
            }
        }

        private void CheckAndSendNotifications()
        {
            double latestPh = GetLatestReading("pH");
            double latestTds = GetLatestReading("TDS");
            double latestTurbidity = GetLatestReading("Turbidity");

            // Check if the latest readings are the same as the previous readings
            if (latestPh == LastReadings["pH"] && latestTds == LastReadings["TDS"] && latestTurbidity == LastReadings["Turbidity"])
            {
                return; // No changes in readings, do not send SMS
            }

            bool smsSent = false; // Flag to track if any SMS has been sent

            if (latestPh < 5 || latestPh > 7)
            {
                string message = $"pH level detected is {(latestPh < 5 ? "LOW" : "HIGH")}: {latestPh} \n\nAcceptable range for pH: 5 - 7\n\nRecommended Action: Check water tank again if it needs cleaning.";
                SendSmsNotification(message);
                smsSent = true; // Mark SMS as sent
            }

            if (latestTds > 10)
            {
                string message = $"TDS level detected is HIGH: {latestTds}\n\nAcceptable range for TDS: <= 10mg/L\n\nRecommended Action: Check water tank again if it needs cleaning.";
                SendSmsNotification(message);
                smsSent = true; // Mark SMS as sent
            }

            if (latestTurbidity > 5)
            {
                string message = $"Turbidity level detected is HIGH: {latestTurbidity}\n\nAcceptable range for turbidity: <= 5.0 NTU\n\nRecommended Action: Check water tank again if it needs cleaning.";
                SendSmsNotification(message);
                smsSent = true; // Mark SMS as sent
            }

            if (smsSent)
            {
                // Store the latest readings
                LastReadings["pH"] = latestPh;
                LastReadings["TDS"] = latestTds;
                LastReadings["Turbidity"] = latestTurbidity;
            }
        }
        private static readonly string ManilaTimeZoneId = "Asia/Manila";
        private static readonly TimeZoneInfo ManilaTimeZone = GetManilaTimeZone();

        private static TimeZoneInfo GetManilaTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(ManilaTimeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback to UTC+8 in case the time zone ID is not found
                return TimeZoneInfo.CreateCustomTimeZone("UTC+08", TimeSpan.FromHours(8), "UTC+08", "UTC+08");
            }
            catch (InvalidTimeZoneException)
            {
                // Handle invalid time zone case
                return TimeZoneInfo.CreateCustomTimeZone("UTC+08", TimeSpan.FromHours(8), "UTC+08", "UTC+08");
            }
        }

        private void ScheduleDailyReports()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 60000; // Check every minute
            timer.Elapsed += (sender, e) =>
            {
                lock (reportLock)
                {
                    DateTime currentTime = DateTime.UtcNow; // Use UTC for consistency

                    // Convert to Philippine time (UTC+8)
                    DateTime philippineTime = TimeZoneInfo.ConvertTimeFromUtc(currentTime, ManilaTimeZone);
                    TimeSpan currentTimeOfDay = philippineTime.TimeOfDay;

                    // Check if current time matches morning report time
                    if (currentTimeOfDay.Hours == morningReportTime.Hours && currentTimeOfDay.Minutes == morningReportTime.Minutes)
                    {
                        if (!morningReportSent)
                        {
                            SendDailyReport();
                            morningReportSent = true;
                            eveningReportSent = false; // Reset evening flag
                        }
                    }
                    else if (currentTimeOfDay.Hours == eveningReportTime.Hours && currentTimeOfDay.Minutes == eveningReportTime.Minutes)
                    {
                        if (!eveningReportSent)
                        {
                            SendDailyReport();
                            eveningReportSent = true;
                            morningReportSent = false; // Reset morning flag
                        }
                    }
                    else
                    {
                        // Reset flags if it's a new day
                        if (currentTimeOfDay.Hours == 0 && currentTimeOfDay.Minutes == 0)
                        {
                            morningReportSent = false;
                            eveningReportSent = false;
                        }
                    }
                }
            };
            timer.Start();
        }

        private void SendDailyReport()
        {
            double latestPh = GetLatestReading("pH");
            double latestTds = GetLatestReading("TDS");
            double latestTurbidity = GetLatestReading("Turbidity");

            string phStatus = GetStatus(latestPh, "pH");
            string tdsStatus = GetStatus(latestTds, "TDS");
            string turbidityStatus = GetStatus(latestTurbidity, "Turbidity");

            string message = $"Daily Water Quality Report:\n\nTDS: {latestTds} {tdsStatus}\npH: {latestPh} {phStatus}\nTurbidity: {latestTurbidity} {turbidityStatus}";
            SendSmsNotification(message);
        }
        private string GetStatus(double value, string parameter)
        {
            switch (parameter)
            {
                case "pH":
                    if (value < 5) return "LOW";
                    if (value > 7) return "HIGH";
                    return "NORMAL";
                case "TDS":
                    return value > 10 ? "HIGH" : "NORMAL";
                case "Turbidity":
                    return value > 5 ? "HIGH" : "NORMAL";
                default:
                    return "NORMAL";
            }
        }

        private double GetLatestReading(string parameter)
        {
            double latestReading = 0.0;

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string query = $"SELECT TOP 1 {parameter} FROM SensorQualityMeasurement ORDER BY MeasurementID DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        latestReading = Convert.ToDouble(result);
                    }
                }
            }

            return latestReading;
        }
        private void SendSmsNotification(string message)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string query = "SELECT Contact FROM Accounts WHERE UserRole IN ('Owner', 'Manager')"; // Filter by UserRole

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string contactNumber = reader.GetString(reader.GetOrdinal("Contact"));
                            // Send SMS notification to each owner and manager
                            SendSmsToContact(message, contactNumber);
                        }
                    }
                }
            }
        }
        private void SendSmsToContact(string message, string contactNumber)
        {
            using (WebClient client = new WebClient())
            {
                // Build the request parameters
                var parameters = new NameValueCollection
                {
                    { "apikey", semaphoreApiKey },
                    { "number", contactNumber }, // Use the contact number from the database
                    { "message", message },
                    { "sendername", "WATERGUARD" },
                };

                try
                {
                    // Make the request to Semaphore API
                    byte[] response = client.UploadValues("https://api.semaphore.co/api/v4/messages", parameters);
                    string result = System.Text.Encoding.UTF8.GetString(response);

                    LogSentMessage();
                }
                catch (WebException ex)
                {
                    // Handle any other types of exceptions
                    Console.WriteLine("An error occurred!");
                    Console.WriteLine("Error message: " + ex.Message);
                }
            }
        }
        private void LogSentMessage()
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string query = "INSERT INTO Events (EventID, DateTime, Message) VALUES (@EventID, @DateTime, 'Notification Sent')";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Generate a unique EventID using GUID
                    Guid eventID = Guid.NewGuid();
                    cmd.Parameters.AddWithValue("@EventID", eventID);
                    cmd.Parameters.AddWithValue("@DateTime", DateTime.Now);

                    // Execute the query
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string GenerateUniqueEventID()
        {
            return Guid.NewGuid().ToString();
        }
        private void BindChart(Chart chart, string baseQuery, string seriesName, string xValueMember)
        {
            string filter = FilterDropDown.SelectedValue;
            DateTime now = DateTime.Now;
            string whereClause = "";

            if (ViewState["SelectedDate"] != null)
            {
                string selectedDate = ViewState["SelectedDate"].ToString();
                whereClause = GetDateFilterClause("MeasurementDateTime", selectedDate, selectedDate);
            }
            else
            {
                switch (filter)
                {
                    case "day":
                        whereClause = GetDateFilterClause("MeasurementDateTime", now.Date.ToString("yyyy-MM-dd"), now.Date.ToString("yyyy-MM-dd"));
                        break;
                    case "week":
                        whereClause = GetDateFilterClause("MeasurementDateTime", now.AddDays(-7).ToString("yyyy-MM-dd"), now.Date.ToString("yyyy-MM-dd"));
                        break;
                    case "month":
                        whereClause = GetDateFilterClause("MeasurementDateTime", now.AddMonths(-1).ToString("yyyy-MM-dd"), now.Date.ToString("yyyy-MM-dd"));
                        break;
                    case "year":
                        whereClause = GetDateFilterClause("MeasurementDateTime", now.AddYears(-1).ToString("yyyy-MM-dd"), now.Date.ToString("yyyy-MM-dd"));
                        break;
                }
            }

            string query = baseQuery;

            if (!string.IsNullOrEmpty(whereClause))
            {
                int groupByIndex = baseQuery.IndexOf("GROUP BY", StringComparison.OrdinalIgnoreCase);
                if (groupByIndex != -1)
                {
                    query = baseQuery.Insert(groupByIndex, " WHERE " + whereClause + " ");
                }
                else
                {
                    query = baseQuery + " WHERE " + whereClause;
                }
            }

            string connectionString = GetConnectionString();
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (ViewState["SelectedDate"] != null)
                    {
                        string selectedDate = ViewState["SelectedDate"].ToString();
                        cmd.Parameters.AddWithValue("@startDate", selectedDate);
                        cmd.Parameters.AddWithValue("@endDate", selectedDate);
                    }
                    else
                    {
                        switch (filter)
                        {
                            case "day":
                                cmd.Parameters.AddWithValue("@startDate", now.Date.ToString("yyyy-MM-dd"));
                                cmd.Parameters.AddWithValue("@endDate", now.Date.ToString("yyyy-MM-dd"));
                                break;
                            case "week":
                                cmd.Parameters.AddWithValue("@startDate", now.AddDays(-7).ToString("yyyy-MM-dd"));
                                cmd.Parameters.AddWithValue("@endDate", now.Date.ToString("yyyy-MM-dd"));
                                break;
                            case "month":
                                cmd.Parameters.AddWithValue("@startDate", now.AddMonths(-1).ToString("yyyy-MM-dd"));
                                cmd.Parameters.AddWithValue("@endDate", now.Date.ToString("yyyy-MM-dd"));
                                break;
                            case "year":
                                cmd.Parameters.AddWithValue("@startDate", now.AddYears(-1).ToString("yyyy-MM-dd"));
                                cmd.Parameters.AddWithValue("@endDate", now.Date.ToString("yyyy-MM-dd"));
                                break;
                        }
                    }

                    conn.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            chart.Series[seriesName].Points.Clear(); // Clear any existing points

            foreach (DataRow row in dt.Rows)
            {
                string xValue = row[xValueMember].ToString();
                int yValue = Convert.ToInt32(row["Count"]);
                DataPoint point = new DataPoint();
                point.AxisLabel = xValue;
                point.YValues = new double[] { yValue };

                // Set colors based on status
                switch (xValue.ToUpper())
                {
                    case "NORMAL":
                    case "ZERO":
                        point.Color = System.Drawing.Color.Green;
                        break;
                    case "HIGH":
                    case "":
                        point.Color = System.Drawing.Color.Red;
                        break;
                    case "LOW":
                        point.Color = System.Drawing.Color.Orange;
                        break;
                }

                chart.Series[seriesName].Points.Add(point);
            }

            chart.Series[seriesName].IsValueShownAsLabel = true; // Optional: Show values on the chart
        }

        private void BindpHChart()
        {
            BindChart(pHChart, "SELECT pHStatus, COUNT(*) AS Count FROM SensorQualityMeasurement GROUP BY pHStatus", "pHSeries", "pHStatus");
        }

        private void BindTDSChart()
        {
            BindChart(TDSChart, "SELECT TDSStatus, COUNT(*) AS Count FROM SensorQualityMeasurement GROUP BY TDSStatus", "TDSSeries", "TDSStatus");
        }

        private void BindTurbidityChart()
        {
            BindChart(TurbidityChart, "SELECT TurbidityStatus, COUNT(*) AS Count FROM SensorQualityMeasurement GROUP BY TurbidityStatus", "TurbiditySeries", "TurbidityStatus");
        }

        private void StartQualityCheckingTimer()
        {
            // Create a new timer with 30 minutes interval
            qualityLoggingTimer = new System.Timers.Timer();
            qualityLoggingTimer.Interval = 30 * 60 * 1000; // 30 minutes in milliseconds
            //qualityLoggingTimer.Interval = 5 * 1000; // 5 seconds in milliseconds
            qualityLoggingTimer.AutoReset = true; // Repeat the timer
            qualityLoggingTimer.Elapsed += QualityLoggingTimer_Elapsed;
            qualityLoggingTimer.Start();
        }
        private void QualityLoggingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Check the quality
            CheckQuality();
        }

        private void CheckQuality()
        {
            double latestPh = GetLatestReading("pH");
            double latestTds = GetLatestReading("TDS");
            double latestTurbidity = GetLatestReading("Turbidity");

            bool phLow = latestPh < 5;
            bool phHigh = latestPh > 7;
            bool tdsHigh = latestTds > 10;
            bool turbidityHigh = latestTurbidity > 5;

            if (phLow || phHigh || tdsHigh || turbidityHigh)
            {
                if (phLow)
                {
                    LogQuality($"PH level detected is LOW: {latestPh}");
                }

                if (phHigh)
                {
                    LogQuality($"PH level detected is HIGH: {latestPh}");
                }

                if (tdsHigh)
                {
                    LogQuality($"TDS level detected is HIGH: {latestTds}");
                }

                if (turbidityHigh)
                {
                    LogQuality($"Turbidity level detected is HIGH: {latestTurbidity}");
                }
            }
            else
            {
                LogQuality($"Water quality is within acceptable range. PH: {latestPh} | TDS: {latestTds} | Turbidity: {latestTurbidity}");
            }
        }
        private void LogQuality(string message)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string query = $"INSERT INTO qualityrepresentation (datetime, description) VALUES (GETDATE(), @Description)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Description", message);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private void BindFormViewDataPH()
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string query = "SELECT TOP 1 pH, pHStatus FROM SensorQualityMeasurement ORDER BY MeasurementID DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet dataSet = new DataSet();
                        adapter.Fill(dataSet);

                        FormView1.DataSource = dataSet.Tables[0];
                        FormView1.DataBind();

                        FormView4.DataSource = dataSet.Tables[0];
                        FormView4.DataBind();

                        // Format data for three decimal places
                        FormatFormViewData(FormView1);
                        FormatFormViewData(FormView4);
                    }
                }
            }
        }

        private void BindFormViewDataTDS()
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string query = "SELECT TOP 1 TDS, TDSStatus FROM SensorQualityMeasurement ORDER BY MeasurementID DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet dataSet = new DataSet();
                        adapter.Fill(dataSet);

                        FormView2.DataSource = dataSet.Tables[0];
                        FormView2.DataBind();

                        FormView5.DataSource = dataSet.Tables[0];
                        FormView5.DataBind();

                        // Format data for three decimal places
                        FormatFormViewData(FormView2);
                        FormatFormViewData(FormView5);
                    }
                }
            }
        }

        private void BindFormViewDataTurbidity()
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                string query = "SELECT TOP 1 Turbidity, TurbidityStatus FROM SensorQualityMeasurement ORDER BY MeasurementID DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet dataSet = new DataSet();
                        adapter.Fill(dataSet);

                        FormView3.DataSource = dataSet.Tables[0];
                        FormView3.DataBind();

                        FormView6.DataSource = dataSet.Tables[0];
                        FormView6.DataBind();

                        // Format data for three decimal places
                        FormatFormViewData(FormView3);
                        FormatFormViewData(FormView6);
                    }
                }
            }
        }

        private void FormatFormViewData(FormView formView)
        {
            if (formView.DataSource != null)
            {
                foreach (DataRow row in ((DataTable)formView.DataSource).Rows)
                {
                    foreach (DataColumn column in ((DataTable)formView.DataSource).Columns)
                    {
                        if (column.DataType == typeof(decimal) || column.DataType == typeof(double) || column.DataType == typeof(float))
                        {
                            row[column] = Math.Round((double)row[column], 3); // Round to three decimal places
                        }
                    }
                }
                formView.DataBind(); // Re-bind the formatted data
            }
        }

        protected string GetMeasureColor(object measureValue)
        {
            if (measureValue != null && !string.IsNullOrEmpty(measureValue.ToString()))
            {
                string measure = measureValue.ToString().ToLowerInvariant();

                if (measure == "normal")
                {
                    return "color: green;";
                }
                else if (measure == "high" || measure == "low")
                {
                    return "color: red;";
                }
            }

            // Default color if the value is not 'safe' or 'unsafe'
            return "color: black;";
        }
        // Helper method to get the column index by name
        private int GetColumnIndexByName(GridView grid, string columnName)
        {
            foreach (DataControlField column in grid.Columns)
            {
                if (column is BoundField && ((BoundField)column).DataField.Equals(columnName))
                {
                    return grid.Columns.IndexOf(column);
                }
            }
            return -1;
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
        private void BindLineChart()
        {
            // Adjust the query to filter data from yesterday and today using TRY_CONVERT to handle date comparison
            string query = @"
SELECT MeasurementDateTime, TDS, pH, Turbidity 
FROM SensorQualityMeasurement 
WHERE 
    (TRY_CONVERT(DATETIME, MeasurementDateTime, 103) >= DATEADD(day, -1, GETDATE()) 
     AND TRY_CONVERT(DATETIME, MeasurementDateTime, 103) < GETDATE())
    OR
    (TRY_CONVERT(DATETIME, MeasurementDateTime, 126) >= DATEADD(day, -1, GETDATE()) 
     AND TRY_CONVERT(DATETIME, MeasurementDateTime, 126) < GETDATE())
    OR
    (TRY_CONVERT(DATETIME, MeasurementDateTime, 105) >= DATEADD(day, -1, GETDATE()) 
     AND TRY_CONVERT(DATETIME, MeasurementDateTime, 105) < GETDATE())
ORDER BY MeasurementDateTime";

            DataTable dt = GetData(query);

            // Clear existing data points
            LineChart.Series["TDS"].Points.Clear();
            LineChart.Series["pH"].Points.Clear();
            LineChart.Series["Turbidity"].Points.Clear();

            // Set the ChartType for each series
            LineChart.Series["TDS"].ChartType = SeriesChartType.Line;
            LineChart.Series["pH"].ChartType = SeriesChartType.Line;
            LineChart.Series["Turbidity"].ChartType = SeriesChartType.Line;

            // Populate chart series with retrieved data
            foreach (DataRow row in dt.Rows)
            {
                if (DateTime.TryParse(row["MeasurementDateTime"].ToString(), out DateTime xValue))
                {
                    double tdsValue = Convert.ToDouble(row["TDS"]);
                    double pHValue = Convert.ToDouble(row["pH"]);
                    double turbidityValue = Convert.ToDouble(row["Turbidity"]);

                    // Add data points to respective series
                    LineChart.Series["TDS"].Points.AddXY(xValue, tdsValue);
                    LineChart.Series["pH"].Points.AddXY(xValue, pHValue);
                    LineChart.Series["Turbidity"].Points.AddXY(xValue, turbidityValue);
                }
                else
                {
                    // Handle the case where the date is not in the expected format
                    // You can log an error, skip the row, etc.
                    Console.WriteLine($"Invalid date format: {row["MeasurementDateTime"]}");
                }
            }

            // Set XValueType to DateTime for correct date-time display
            LineChart.Series["TDS"].XValueType = ChartValueType.DateTime;
            LineChart.Series["pH"].XValueType = ChartValueType.DateTime;
            LineChart.Series["Turbidity"].XValueType = ChartValueType.DateTime;

            // Set chart area properties for date-time display
            LineChart.ChartAreas["LineChartArea1"].AxisX.LabelStyle.Format = "dd/MM/yyyy HH:mm";
            LineChart.ChartAreas["LineChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Hours;
            LineChart.ChartAreas["LineChartArea1"].AxisX.Interval = 1;

            // Set background color
            LineChart.ChartAreas["LineChartArea1"].BackColor = Color.White;

            // Recalculate axes scale
            LineChart.ChartAreas["LineChartArea1"].RecalculateAxesScale();
        }

        private DataTable GetData(string query)
        {
            DataTable dt = new DataTable();
            string connectionString = GetConnectionString(); // Replace with your actual connection string

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            return dt;
        }



        protected void FilterDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGridView();
            BindpHChart();
            BindTDSChart();
            BindTurbidityChart();
            BindLineChart();
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
            }
            else if (!string.IsNullOrEmpty(startDate))
            {
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 103) = @startDate"); // dd/MM/yyyy
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 101) = @startDate"); // MM/dd/yyyy
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 120) = @startDate"); // yyyy-MM-dd

                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 121) = @startDate"); // yyyy-MM-dd HH:mm:ss.mmm
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 126) = @startDate"); // ISO 8601
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 112) = @startDate"); // yyyymmdd
            }
            else if (!string.IsNullOrEmpty(endDate))
            {
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 103) <= @endDate");
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 101) <= @endDate");
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 120) <= @endDate");

                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 121) <= @endDate");
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 126) <= @endDate");
                dateFilters.Add($"TRY_CONVERT(date, {columnName}, 112) <= @endDate");
            }

            // Combine filters with OR for flexibility
            return string.Join(" OR ", dateFilters);
        }
        protected void BindGridView(string selectedDate = null)
        {
            string filter = FilterDropDown.SelectedValue;
            DateTime now = DateTime.Now;
            string whereClause = "";

            if (!string.IsNullOrEmpty(selectedDate))
            {
                whereClause = " WHERE " + GetDateFilterClause("MeasurementDateTime", selectedDate, selectedDate);
            }
            else
            {
                switch (filter)
                {
                    case "day":
                        whereClause = " WHERE " + GetDateFilterClause("MeasurementDateTime", now.Date.ToString("yyyy-MM-dd"), now.Date.ToString("yyyy-MM-dd"));
                        break;
                    case "week":
                        whereClause = " WHERE " + GetDateFilterClause("MeasurementDateTime", now.AddDays(-7).ToString("yyyy-MM-dd"), now.Date.ToString("yyyy-MM-dd"));
                        break;
                    case "month":
                        whereClause = " WHERE " + GetDateFilterClause("MeasurementDateTime", now.AddMonths(-1).ToString("yyyy-MM-dd"), now.Date.ToString("yyyy-MM-dd"));
                        break;
                    case "year":
                        whereClause = " WHERE " + GetDateFilterClause("MeasurementDateTime", now.AddYears(-1).ToString("yyyy-MM-dd"), now.Date.ToString("yyyy-MM-dd"));
                        break;
                }
            }

            string query = "SELECT * FROM SensorQualityMeasurement" + whereClause + " ORDER BY MeasurementID DESC";

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(selectedDate))
                    {
                        cmd.Parameters.AddWithValue("@startDate", selectedDate);
                        cmd.Parameters.AddWithValue("@endDate", selectedDate);
                    }
                    else
                    {
                        switch (filter)
                        {
                            case "day":
                                cmd.Parameters.AddWithValue("@startDate", now.Date.ToString("yyyy-MM-dd"));
                                cmd.Parameters.AddWithValue("@endDate", now.Date.ToString("yyyy-MM-dd"));
                                break;
                            case "week":
                                cmd.Parameters.AddWithValue("@startDate", now.AddDays(-7).ToString("yyyy-MM-dd"));
                                cmd.Parameters.AddWithValue("@endDate", now.Date.ToString("yyyy-MM-dd"));
                                break;
                            case "month":
                                cmd.Parameters.AddWithValue("@startDate", now.AddMonths(-1).ToString("yyyy-MM-dd"));
                                cmd.Parameters.AddWithValue("@endDate", now.Date.ToString("yyyy-MM-dd"));
                                break;
                            case "year":
                                cmd.Parameters.AddWithValue("@startDate", now.AddYears(-1).ToString("yyyy-MM-dd"));
                                cmd.Parameters.AddWithValue("@endDate", now.Date.ToString("yyyy-MM-dd"));
                                break;
                        }
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet dataSet = new DataSet();
                        adapter.Fill(dataSet);

                        if (dataSet.Tables[0].Rows.Count > 0)
                        {
                            GridView1.DataSource = dataSet.Tables[0];
                            GridView1.DataBind();
                        }
                        else
                        {
                            GridView1.DataSource = null;
                            GridView1.DataBind();
                        }
                    }
                }
            }
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            // Set the new page index and rebind the GridView
            GridView1.PageIndex = e.NewPageIndex;
            BindGridView();
        }
        protected void FormView1_PageIndexChanging(object sender, FormViewPageEventArgs e)
        {
            BindFormViewDataPH();
        }
        protected void FormView2_PageIndexChanging(object sender, FormViewPageEventArgs e)
        {
            BindFormViewDataTDS();
        }
        protected void FormView3_PageIndexChanging(object sender, FormViewPageEventArgs e)
        {
            BindFormViewDataTurbidity();
        }
        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGridView();
        }
    }
}
