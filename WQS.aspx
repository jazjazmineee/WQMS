<%@ Page Title="" Language="C#" MasterPageFile="~/Sidebar.Master" AutoEventWireup="true" CodeBehind="WQS.aspx.cs" Inherits="WaterGuard.WQS" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <!-- Bootstrap CSS -->
    <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css">

    <!-- DataTables CSS and JS -->
    <link rel="stylesheet" href="https://cdn.datatables.net/1.10.21/css/dataTables.bootstrap4.min.css">
    <script src="https://code.jquery.com/jquery-3.5.1.min.js"></script>
    <script src="https://cdn.datatables.net/1.10.21/js/jquery.dataTables.min.js"></script>
    <script src="https://cdn.datatables.net/1.10.21/js/dataTables.bootstrap4.min.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="main p-4 mt-5">
        <!-- Page Heading -->
        <div class="d-sm-flex align-items-center justify-content-between mb-4">
            <h1 class="h1 mb-0 text-gray-800">Water Quality Standard</h1>
        </div>

        <div class="col-lg-6 mb-4">
            <!-- Illustrations -->
            <div class="card shadow mb-4">
                <div class="card-header py-3">
                    <h6 class="m-0 font-weight-bold text-primary">Based on Philippines National Standard for Drinking Water</h6>
                </div>
                <div class="card-body">
                    <div class="table-responsive">
                        <table class="table table-lg table-bordered" id="dataTable" width="100%" cellspacing="0">
                            <thead>
                                <tr>
                                    <th>Parameter</th>
                                    <th>Standard Value</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td>Total Dissolved Solids (TDS)</td>
                                    <td>10 mg/L</td>
                                </tr>
                                <tr>
                                    <td>pH</td>
                                    <td>5 - 7</td>
                                </tr>
                                <tr>
                                    <td>Turbidity</td>
                                    <td>5.0 NTU</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-6 mb-4">
            <div class="card shadow mb-4">
                <div class="card-header py-3">
                    <h6 class="m-0 font-weight-bold text-primary">References</h6>
                </div>
                <div class="card-body">
                    <p>
                        Link 1: <a href="https://www.fda.gov.ph/wp-content/uploads/2020/10/Administrative-Order-No.-2017-0010.pdf" target="_blank">Administrative Order No. 2017 0010</a>.<br />
                        Link 2: <a href="https://iwaponline.com/jwh/article/15/2/288/28272/Updating-national-standards-for-drinking-water-a" target="_blank">Mandatory Drinking-Water Quality Parameters and their Standard Values</a>.
                    </p>
                </div>
            </div>
        </div>



    </div>



    <!-- Content Row -->

</asp:Content>
