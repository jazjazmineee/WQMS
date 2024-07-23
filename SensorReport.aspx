﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Sidebar.Master" AutoEventWireup="true" CodeBehind="SensorReport.aspx.cs" Inherits="WaterGuard_2024.SensorReport" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
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
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>

    <!-- daterangepicker CSS and JS -->
    <link rel="stylesheet" type="text/css" href="https://cdn.jsdelivr.net/npm/daterangepicker/daterangepicker.css" />
    <script type="text/javascript" src="https://cdn.jsdelivr.net/momentjs/latest/moment.min.js"></script>
    <script type="text/javascript" src="https://cdn.jsdelivr.net/jquery/latest/jquery.min.js"></script>
    <script type="text/javascript" src="https://cdn.jsdelivr.net/npm/daterangepicker/daterangepicker.min.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="main p-4 mt-5">
        <!-- Page Heading -->
        <div class="d-sm-flex align-items-center justify-content-between mb-4">
            <h1 class="h1 mb-0 text-gray-800">Sensor Report</h1>
        </div>


        <div class="row col-lg-12">
            <div class="col-lg-6 mb-4">
                <div class="input-group input-group-sm">
                    <span class="input-group-text">Select Date:</span>
                    <asp:TextBox ID="singleDateRange" runat="server" CssClass="daterange" />
                    <asp:Button ID="Button3" runat="server" class="btn btn-primary" data-bs-dismiss="modal" Text="Search" OnClick="SingleSearch_Click" />
                </div>
            </div>
            <div class="col-lg-6 mb-4">
                <div class="input-group input-group-sm">
                    <span class="input-group-text">Select Date Range:</span>
                    <asp:TextBox ID="daterange" runat="server" CssClass="daterange" />
                    <asp:Button ID="Button2" runat="server" class="btn btn-primary" data-bs-dismiss="modal" Text="Search" OnClick="Search_Click" />
                </div>
            </div>
        </div>

        <div class="col-lg-12 mb-4">
            <div class="alert alert-danger d-none mb-2" id="error_alert" role="alert"></div>
        </div>


        <!-- Table -->
        <div class="col-lg-12 mb-4">
            <div class="card mb-4">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <div>
                        <asp:LinkButton ID="DownloadPDFLinkButton" runat="server" Font-Size="Large" Text="Download PDF |" OnClick="DownloadPDFLinkButton_Click" />
                        <asp:LinkButton ID="ExportBtn" runat="server" Text="Export to Excel" Font-Size="Large" OnClick="ExportBtn_Click" />
                    </div>
                    <div>
                        <a href="#" data-bs-toggle="modal" data-bs-target="#filterdModal" style="font-size: large">
                            <i class='bx bx-plus-circle'></i>Add Sensor Filter&nbsp;&nbsp;
                        </a>
                        <asp:LinkButton ID="ShowAllDatesLinkButton" runat="server" Text="Show All" OnClick="ShowAllDates_Click" Font-Size="Large" />
                    </div>
                </div>

                <div class="card-body">
                    <div class="table-responsive">
                        <table class="table">
                            <asp:GridView ID="GridView1" runat="server" EnableViewState="true" AutoGenerateColumns="False" OnPageIndexChanging="GridView1_PageIndexChanging"
                                CssClass="table table-bordered text-center" Style="width: 100%" PageSize="10" AllowPaging="true" OnRowDataBound="GridView1_RowDataBound">
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
    </div>

    <!-- Add Filter Modal -->
    <div class="modal fade" id="filterdModal" tabindex="-1" aria-labelledby="filterdModallLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header text-">
                    <h5 class="modal-title" id="filterdModalLabel">Add Sensor Filter</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <asp:DropDownList ID="DropDownList1" runat="server" CssClass="form-select">
                            <asp:ListItem Value="">Select Sensor Type</asp:ListItem>
                            <asp:ListItem Value="TDS">TDS</asp:ListItem>
                            <asp:ListItem Value="pH">pH</asp:ListItem>
                            <asp:ListItem Value="Turbidity">Turbidity</asp:ListItem>
                        </asp:DropDownList>

                    </div>
                    <div class="mb-3">
                        <asp:DropDownList ID="DropDownList2" runat="server" CssClass="form-select">
                            <asp:ListItem Value="">Select Status</asp:ListItem>
                            <asp:ListItem Value="NORMAL">NORMAL</asp:ListItem>
                            <asp:ListItem Value="HIGH">HIGH</asp:ListItem>
                            <asp:ListItem Value="LOW">LOW</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="filterBtn" runat="server" class="btn btn-primary" Text="Add Filter" OnClick="filterBtn_Click" ValidationGroup="GroupFilter" />
                </div>
            </div>
        </div>
    </div>

    <script>
        $(function () {
            // Detect device's locale and set date format accordingly
            var deviceLocale = navigator.language || navigator.browserLanguage;
            var dateFormat = deviceLocale.includes('en') ? 'MM/DD/YYYY' : 'DD/MM/YYYY';

            $('#<%= daterange.ClientID %>').daterangepicker({
                opens: 'left',
                autoApply: true,
                locale: {
                    format: dateFormat
                }
            }, function (start, end, label) {
                $('#<%= daterange.ClientID %>').val(start.format(dateFormat) + ' - ' + end.format(dateFormat));
            });
            $('#<%= daterange.ClientID %>').val('');

            $('#<%= singleDateRange.ClientID %>').daterangepicker({
                singleDatePicker: true,
                opens: 'left',
                autoApply: true,
                locale: {
                    format: dateFormat
                }
            }, function (start, end, label) {
                $('#<%= singleDateRange.ClientID %>').val(start.format(dateFormat));
            });
            $('#<%= singleDateRange.ClientID %>').val('');
        });

        function showAlertAndHide(message) {
            $('#error_alert').text(message); // Set the text of the error alert
            $('#error_alert').removeClass('d-none').addClass('show');
            setTimeout(function () {
                $('#error_alert').removeClass('show').addClass('d-none');
            }, 5000); // Hide alert after 5 seconds
        }

    </script>


</asp:Content>

