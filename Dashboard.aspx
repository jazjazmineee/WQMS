<%@ Page Title="" Language="C#" MasterPageFile="~/Sidebar.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="WaterGuard_2024.Dashboard" %>

<asp:Content ID="Content4" ContentPlaceHolderID="head" runat="server">
    <style>
        /* CSS to color the header cells */
        .table th {
            background-color: #EFEFEF; /* Change the background color as needed */
            color: #000; /* Change the text color as needed */
        }
    </style>

    <!-- Bootstrap CSS -->
    <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css">

    <!-- DataTables CSS and JS -->
    <link rel="stylesheet" href="https://cdn.datatables.net/1.10.21/css/dataTables.bootstrap4.min.css">
    <script src="https://code.jquery.com/jquery-3.5.1.min.js"></script>
    <script src="https://cdn.datatables.net/1.10.21/js/jquery.dataTables.min.js"></script>
    <script src="https://cdn.datatables.net/1.10.21/js/dataTables.bootstrap4.min.js"></script>
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <div class="main p-4 mt-5">
        <!-- Page Heading -->
        <div class="d-sm-flex align-items-center justify-content-between mb-4">
            <h1 class="h1 mb-0 text-gray-800">Dashboard</h1>
            <div>
                <asp:DropDownList ID="FilterDropDown" runat="server" CssClass="form-select" AutoPostBack="True" OnSelectedIndexChanged="FilterDropDown_SelectedIndexChanged">
                    <asp:ListItem Value="day">Day</asp:ListItem>
                    <asp:ListItem Value="week">Week</asp:ListItem>
                    <asp:ListItem Value="month">Month</asp:ListItem>
                    <asp:ListItem Value="year">Year</asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>

        <!-- Content Row -->
        <div class="row">


            <!-- TDS LEVEL -->
            <div class="col-lg-4 col-md-6 mx-auto mb-4">
                <div class="card shadow w-100">
                    <div class="card-body">
                        <div class="row no-gutters align-items-center">
                            <div class="col mr-2">
                                <%-- 1 --%>
                                <div class="fs-5 text-secondary text-uppercase mb-1">
                                    TDS
                                </div>
                                <%-- 2 --%>
                                <div class="h1 mb-0 font-weight-bold text-gray-800">
                                    <asp:FormView ID="FormView2" runat="server" OnPageIndexChanging="FormView2_PageIndexChanging">
                                        <ItemTemplate>
                                            <p class="lvl-values"><%# Eval("TDS") %></p>
                                        </ItemTemplate>
                                    </asp:FormView>
                                </div>
                            </div>
                            <%-- 3 --%>
                            <div class="h5 col-auto mb-0 font-weight-bold mt-auto text-right fs-2 text-uppercase">
                                <asp:FormView ID="FormView5" runat="server">
                                    <ItemTemplate>
                                        <p style='<%# GetMeasureColor(Eval("TDSStatus")) %>'>
                                            <%# Eval("TDSStatus") %>
                                        </p>
                                    </ItemTemplate>
                                </asp:FormView>
                            </div>
                        </div>
                        <div class="row no-gutters align-items-center mt-3">
                            <div class="col">
                                <small><a class="fs-6 text-secondary">Latest Reading</a></small>
                            </div>
                            <div class="col-auto text-right">
                                <asp:Label ID="LabelTDSMessage" runat="server" CssClass="text-danger" Visible="false" Style="font-size: 13px;">Check the water tank if it needs cleaning</asp:Label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- PH LEVEL -->
            <div class="col-lg-4 col-md-6 mx-auto mb-4">
                <div class="card shadow w-100">
                    <div class="card-body">
                        <div class="row no-gutters align-items-center">
                            <div class="col mr-2">
                                <%-- 1 --%>
                                <div class="fs-5 text-secondary text-uppercase mb-1">
                                    pH 
                                </div>
                                <%-- 2 --%>
                                <div class="h1 mb-0 font-weight-bold text-gray-800">
                                    <asp:FormView ID="FormView1" runat="server" OnPageIndexChanging="FormView1_PageIndexChanging">
                                        <ItemTemplate>
                                            <p class="lvl-values"><%# Eval("pH") %></p>
                                        </ItemTemplate>
                                    </asp:FormView>
                                </div>
                            </div>
                            <%-- 3 --%>
                            <div class="h5 col-auto mb-0 font-weight-bold mt-auto text-right fs-2 text-uppercase">
                                <asp:FormView ID="FormView4" runat="server">
                                    <ItemTemplate>
                                        <p style='<%# GetMeasureColor(Eval("pHStatus")) %>'>
                                            <%# Eval("pHStatus") %>
                                        </p>
                                    </ItemTemplate>
                                </asp:FormView>
                            </div>
                        </div>
                        <div class="row no-gutters align-items-center mt-3">
                            <div class="col">
                                <small><a class="fs-6 text-secondary">Latest Reading</a></small>
                            </div>
                            <div class="col-auto text-right">
                                <asp:Label ID="LabelPHMessage" runat="server" CssClass="text-danger" Visible="false" Style="font-size: 13px;">Check the water tank if it needs cleaning</asp:Label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- TURBIDITY LEVEL -->
            <div class="col-lg-4 col-md-6 mx-auto mb-4">
                <div class="card shadow w-100">
                    <div class="card-body">
                        <div class="row no-gutters align-items-center">
                            <div class="col mr-2">
                                <%-- 1 --%>
                                <div class="fs-5 text-secondary text-uppercase mb-1">
                                    Turbidity
                                </div>
                                <%-- 2 --%>
                                <div class="h1 mb-0 font-weight-bold text-gray-800">
                                    <asp:FormView ID="FormView3" runat="server" OnPageIndexChanging="FormView3_PageIndexChanging">
                                        <ItemTemplate>
                                            <p class="lvl-values"><%# Eval("Turbidity") %></p>
                                        </ItemTemplate>
                                    </asp:FormView>
                                </div>
                            </div>
                            <%-- 3 --%>
                            <div class="h5 col-auto mb-0 font-weight-bold mt-auto text-right fs-2 text-uppercase">
                                <asp:FormView ID="FormView6" runat="server">
                                    <ItemTemplate>
                                        <p style='<%# GetMeasureColor(Eval("TurbidityStatus")) %>'>
                                            <%# Eval("TurbidityStatus") %>
                                        </p>
                                    </ItemTemplate>
                                </asp:FormView>
                            </div>
                        </div>
                        <div class="row no-gutters align-items-center mt-3">
                            <div class="col">
                                <small><a class="fs-6 text-secondary">Latest Reading</a></small>
                            </div>
                            <div class="col-auto text-right">
                                <asp:Label ID="LabelTurbidityMessage" runat="server" CssClass="text-danger" Visible="false" Style="font-size: 13px;">Check the water tank if it needs cleaning</asp:Label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>


            <!-- Doughnut Chart -->
            <div class="col-lg-4 col-md-6 mx-auto mb-4">
                <div class="card shadow w-100">
                    <div class="card-header">
                        <h6 class="font-weight-bold">TDS</h6>
                    </div>
                    <div class="card-body">
                        <p class="fs-5 text-secondary text-uppercase mb-1" style="margin: 5px; font-size: 16px;">
                            <span style="color: green;">●</span> Normal &nbsp;&nbsp;&nbsp;&nbsp; 
                <span style="color: red;">●</span> High &nbsp;&nbsp;&nbsp;&nbsp;
                        </p>
                        <asp:Chart ID="TDSChart" runat="server" Width="250" Height="250" Palette="Pastel" EnableViewState="True">
                            <Series>
                                <asp:Series Name="TDSSeries" ChartArea="ChartArea1" ChartType="Doughnut">
                                </asp:Series>
                            </Series>
                            <ChartAreas>
                                <asp:ChartArea Name="ChartArea1">
                                </asp:ChartArea>
                            </ChartAreas>
                        </asp:Chart>
                    </div>
                </div>

            </div>

            <div class="col-lg-4 col-md-6 mx-auto mb-4">
                <div class="card shadow w-100">
                    <div class="card-header">
                        <h6 class="font-weight-bold">pH</h6>
                    </div>
                    <div class="card-body">
                        <p class="fs-5 text-secondary text-uppercase mb-1" style="margin: 5px; font-size: 16px;">
                            <span style="color: green;">●</span> Normal &nbsp;&nbsp;&nbsp;&nbsp; 
                 <span style="color: red;">●</span> High &nbsp;&nbsp;&nbsp;&nbsp;
                  <span style="color: orange;">●</span> Low &nbsp;&nbsp;&nbsp;&nbsp;
                        </p>
                        <asp:Chart ID="pHChart" runat="server" Width="250" Height="250" EnableViewState="True">
                            <Series>
                                <asp:Series Name="pHSeries" ChartArea="ChartArea2" ChartType="Doughnut">
                                </asp:Series>
                            </Series>
                            <ChartAreas>
                                <asp:ChartArea Name="ChartArea2">
                                </asp:ChartArea>
                            </ChartAreas>
                        </asp:Chart>
                    </div>
                </div>
            </div>

            <div class="col-lg-4 col-md-6 mx-auto mb-4 ">
                <div class="card shadow w-100">
                    <div class="card-header">
                        <h6 class="font-weight-bold">Turbidity</h6>
                    </div>
                    <div class="card-body">
                        <p class="fs-5 text-secondary text-uppercase mb-1" style="margin: 5px; font-size: 16px;">
                            <span style="color: green;">●</span> Normal &nbsp;&nbsp;&nbsp;&nbsp; 
                        <span style="color: red;">●</span> High &nbsp;&nbsp;&nbsp;&nbsp;
                        </p>
                        <asp:Chart ID="TurbidityChart" runat="server" Width="250" Height="250" EnableViewState="True">
                            <Series>
                                <asp:Series Name="TurbiditySeries" ChartArea="ChartArea3" ChartType="Doughnut">
                                </asp:Series>
                            </Series>
                            <ChartAreas>
                                <asp:ChartArea Name="ChartArea3">
                                </asp:ChartArea>
                            </ChartAreas>
                        </asp:Chart>
                    </div>
                </div>

            </div>
            <!-- Line Chart -->
            <div>
                <asp:Chart ID="LineChart" runat="server" Width="1500" Height="800" BackColor="White">
                    <Series>
                        <asp:Series Name="TDS" ChartType="Spline" Color="Blue" IsVisibleInLegend="False"></asp:Series>
                        <asp:Series Name="pH" ChartType="Spline" Color="Purple" IsVisibleInLegend="False"></asp:Series>
                        <asp:Series Name="Turbidity" ChartType="Spline" Color="Pink" IsVisibleInLegend="False"></asp:Series>
                    </Series>
                    <ChartAreas>
                        <asp:ChartArea Name="LineChartArea1">
                            <AxisX Title="Time" IntervalType="Days" Interval="1"></AxisX>
                            <AxisY Title="Value"></AxisY>
                        </asp:ChartArea>
                    </ChartAreas>
                    <Legends>
                        <asp:Legend Name="Default" BackColor="White" ForeColor="Black" Font="Arial, 12px, style=Bold">
                            <CustomItems>
                                <asp:LegendItem Name="TDS" Color="Blue" BorderColor="Black" MarkerStyle="Square" MarkerSize="10" MarkerColor="Blue" />
                                <asp:LegendItem Name="pH" Color="Purple" BorderColor="Black" MarkerStyle="Square" MarkerSize="10" MarkerColor="Purple" />
                                <asp:LegendItem Name="Turbidity" Color="Pink" BorderColor="Black" MarkerStyle="Square" MarkerSize="10" MarkerColor="Pink" />
                            </CustomItems>
                        </asp:Legend>
                    </Legends>
                </asp:Chart>
            </div>



            <!-- Table -->

            <div class="card shadow mb-4">
                <div class="card-body">
                    <div class="table-responsive">
                        <table class="table">
                            <asp:GridView ID="GridView1" runat="server" EnableViewState="true" AutoGenerateColumns="False" OnPageIndexChanging="GridView1_PageIndexChanging"
                                CssClass="table table-bordered text-center" Style="width: 100%" PageSize="8" AllowPaging="true" OnRowDataBound="GridView1_RowDataBound">
                                <Columns>
                                    <asp:BoundField DataField="MeasurementDateTime" HeaderText="Date & Time" SortExpression="MeasurementDateTime" />
                                    <asp:BoundField DataField="TDS" HeaderText="TDS" SortExpression="TDS" />
                                    <asp:BoundField DataField="TDSStatus" HeaderText="TDS Status" SortExpression="TDSStatus" />
                                    <asp:BoundField DataField="pH" HeaderText="pH" SortExpression="pH" />
                                    <asp:BoundField DataField="pHStatus" HeaderText="pH Status" SortExpression="pHStatus" />
                                    <asp:BoundField DataField="Turbidity" HeaderText="Turbidity" SortExpression="Turbidity" />
                                    <asp:BoundField DataField="TurbidityStatus" HeaderText="Turbidity Status" SortExpression="TurbidityStatus" />
                                </Columns>
                                <EmptyDataTemplate>
                                    <div class="alert alert-info text-center">
                                        No data found.
                                    </div>
                                </EmptyDataTemplate>
                                <RowStyle />
                                <RowStyle HorizontalAlign="Center" />
                                <PagerSettings Mode="Numeric" Position="Bottom" />
                                <PagerStyle HorizontalAlign="Center" />
                            </asp:GridView>
                        </table>
                    </div>
                </div>
            </div>



        </div>




        <asp:Timer ID="Timer1" runat="server" Interval="300000" OnTick="updateDashboard"></asp:Timer>
        <%-- 30 minutes = 30 * 60 * 1000 milliseconds = 1800000 --%>

        <%--<asp:Timer ID="Timer1" runat="server" Interval="600000" OnTick="updateDashboard"></asp:Timer>
        <%--10 mins 600000--%>--%>
    </div>

</asp:Content>
