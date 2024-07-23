<%@ Page Title="" Language="C#" MasterPageFile="~/Sidebar.Master" AutoEventWireup="true" CodeBehind="AccManagement.aspx.cs" Inherits="WaterGuard_2024.AccManagement" %>

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

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="main p-4 mt-5">
        <!-- Page Heading -->
        <div class="d-sm-flex align-items-center justify-content-between mb-4">
            <h1 class="h1 mb-0 text-gray-800">Account Management</h1>
        </div>

        <div class="row">
            <div class="col-lg-6 mb-4">
                <div class="ps-3">
                    <div class="btn-toolbar" role="toolbar">
                        <div class="btn-group me-2" role="group">
                            <asp:Button CssClass="btn btn-primary" ID="addbtn1" runat="server" Text="ADD ACCOUNT" OnClientClick="openAddUserModal(); return false;" />
                        </div>
                        <div class="btn-group me-2" role="group">
                            <asp:Button CssClass="btn btn-warning" ID="updatebtn1" runat="server" Text="UPDATE ACCOUNT" OnClientClick="openUpdateUserModal(); return false;" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="alert alert-danger d-none mb-2" id="errorAlert" role="alert">
        </div>
        <div class="alert alert-success d-none mb-2" id="successAlert" role="alert">
        </div>
        <div class="row">
            <!-- Table -->
            <div class="col-lg-12 mb-4">
                <div class="card mb-4">
                    <div class="card-header d-flex justify-content-end">
                        <a href="#" data-bs-toggle="modal" data-bs-target="#accountfilterdModal" style="font-size: large">
                            <i class='bx bx-plus-circle'></i>
                            Add Status Filter</a>&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:LinkButton ID="ShowAllAccount" runat="server" Text="Show All" OnClick="ShowAllAccount_Click" Font-Size="Large" />
                    </div>
                    <div class="card-body">
                        <div class="table-responsive">
                            <table class="table">
                                <asp:GridView ID="GridView1" runat="server" EnableViewState="true" AutoGenerateColumns="False" OnRowDataBound="GridView1_RowDataBound" OnPageIndexChanging="GridView1_PageIndexChanging"
                                    CssClass="table table-bordered text-center" Style="width: 100%" PageSize="10" AllowPaging="true">
                                    <Columns>
                                        <asp:BoundField DataField="UserID" HeaderText="User ID" SortExpression="UserID" />
                                        <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                                        <asp:BoundField DataField="Username" HeaderText="Username" SortExpression="Username" />
                                        <asp:BoundField DataField="Contact" HeaderText="Contact" SortExpression="Contact" />
                                        <asp:BoundField DataField="UserRole" HeaderText="Role" SortExpression="UserRole" />
                                        <asp:BoundField DataField="Status" HeaderText="Status" SortExpression="Status" />
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
    </div>


    <!-- Add Account Filter Modal -->
    <div class="modal fade" id="accountfilterdModal" tabindex="-1" aria-labelledby="accountfilterdModallLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header text-">
                    <h5 class="modal-title" id="filterdModalLabel">Add Status Filter</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <asp:DropDownList ID="DropDownList1" runat="server" CssClass="form-select">
                            <asp:ListItem Value="">Select Status</asp:ListItem>
                            <asp:ListItem Value="Active">ACTIVE</asp:ListItem>
                            <asp:ListItem Value="Inactive">INACTIVE</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="AccountFilterBtn" runat="server" class="btn btn-primary" Text="Add Filter" OnClick="AccountFilterBtn_Click" ValidationGroup="GroupFilter" />
                </div>
            </div>
        </div>
    </div>


    <!-- Add User Modal -->
    <div class="modal fade" id="addUserModal" aria-labelledby="addUserModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="addUserModalLabel">Add New User</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close" onclick="clearFields()"></button>
                </div>
                <div class="modal-body">
                    <!-- Name -->
                    <div class="mb-2">
                        <label for="name" class="form-label">Name</label>
                        <asp:TextBox ID="name" runat="server" type="text" class="form-control" name="name"></asp:TextBox>
                    </div>
                    <!-- Username -->
                    <div class="mb-2">
                        <label for="userName" class="form-label">Username</label>
                        <asp:TextBox ID="userName" runat="server" type="text" class="form-control" name="userName"></asp:TextBox>
                    </div>
                    <!-- Role -->
                    <div class="mb-2">
                        <label for="userRole" class="form-label">Role</label>
                        <asp:DropDownList ID="userRole" runat="server" CssClass="form-select custom-select" name="userRole">
                            <asp:ListItem Value=""></asp:ListItem>
                            <asp:ListItem Value="Manager">Manager</asp:ListItem>
                            <asp:ListItem Value="Staff">Staff</asp:ListItem>
                            <asp:ListItem Value="Owner">Owner</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <!-- Contact -->
                    <div class="mb-2">
                        <label for="contact" class="form-label">Contact</label>
                        <asp:TextBox ID="contact" runat="server" type="text" class="form-control" name="contact"></asp:TextBox>
                    </div>
                    <!-- Status -->
                    <div class="mb-3">
                        <label for="status" class="form-label">Status</label>
                        <asp:DropDownList ID="status" runat="server" CssClass="form-select custom-select" name="status">
                            <asp:ListItem Value=""></asp:ListItem>
                            <asp:ListItem Value="ACTIVE">ACTIVE</asp:ListItem>
                            <asp:ListItem Value="INACTIVE">INACTIVE</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <!-- Alert -->
                    <div class="mb-2">
                        <div class="alert alert-danger d-none mb-2" id="error_add" role="alert" style="width: 100%;"></div>
                    </div>
                </div>

                <div class="modal-footer">
                    <asp:Button ID="AddButton" runat="server" class="btn btn-primary" Text="Add User" OnClientClick="return validateAdd();" OnClick="AddButton_Click" />
                </div>
            </div>
        </div>
    </div>

    <!-- Update User Modal -->
    <div class="modal fade" id="updateUserModal" aria-labelledby="updateUserModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="updateUserModalLabel">Update User</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close" onclick="clearFields()"></button>
                </div>
                <div class="modal-body">
                    <!-- Name -->
                    <div class="mb-2">
                        <label for="udName" class="form-label">Name</label>
                        <div class="input-group">
                            <asp:TextBox ID="udName" runat="server" type="text" class="form-control" name="udName"></asp:TextBox>
                            <asp:Button ID="SearchButton" CssClass="btn btn-outline-secondary" runat="server" Text="Search" OnClientClick="return validateSearch();" OnClick="SearchButton_Click" />
                        </div>
                    </div>
                    <!-- Username -->
                    <div class="mb-2">
                        <label for="udUserName" class="form-label">Username</label>
                        <asp:TextBox ID="udUserName" runat="server" type="text" class="form-control" name="udUserName"></asp:TextBox>
                    </div>
                    <!-- Role -->
                    <div class="mb-2">
                        <label for="udUserRole" class="form-label">Role</label>
                        <asp:DropDownList ID="udUserRole" runat="server" CssClass="form-select custom-select" name="udUserRole">
                            <asp:ListItem Value=""></asp:ListItem>
                            <asp:ListItem Value="Manager">Manager</asp:ListItem>
                            <asp:ListItem Value="Staff">Staff</asp:ListItem>
                            <asp:ListItem Value="Owner">Owner</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <!-- Status -->
                    <div class="mb-3">
                        <label for="udStatus" class="form-label">Status</label>
                        <asp:DropDownList ID="udStatus" runat="server" CssClass="form-select custom-select" name="udStatus">
                            <asp:ListItem Value=""></asp:ListItem>
                            <asp:ListItem Value="ACTIVE">ACTIVE</asp:ListItem>
                            <asp:ListItem Value="INACTIVE">INACTIVE</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <!-- Alert -->
                    <div class="mb-2">
                        <div class="alert alert-danger d-none mb-2" id="error_update" role="alert" style="width: 100%;"></div>
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="UdButton" runat="server" class="btn btn-warning" Text="Update User" OnClientClick="return validateUpdate();" OnClick="UdButton_Click" />
                </div>
            </div>
        </div>
    </div>


    <!-- Client-side validation script -->
    <script>

        function clearFields() {
            document.getElementById('<%= name.ClientID %>').value = '';
            document.getElementById('<%= userName.ClientID %>').value = '';
            document.getElementById('<%= userRole.ClientID %>').value = '';
            document.getElementById('<%= contact.ClientID %>').value = '';
            document.getElementById('<%= status.ClientID %>').value = '';

            document.getElementById('<%= udName.ClientID %>').value = '';
            document.getElementById('<%= udUserName.ClientID %>').value = '';
            document.getElementById('<%= udUserRole.ClientID %>').value = '';
            document.getElementById('<%= udStatus.ClientID %>').value = '';
        }

        function openAddUserModal() {
            $('#addUserModal').modal('show');
        }

        function openUpdateUserModal() {
            $('#updateUserModal').modal('show');
        }

        function showUpdateUserModal() {
            $(document).ready(function () {
                $('#updateUserModal').modal('show');
            });
        }

        function validateAdd() {
            var name = document.getElementById('<%= name.ClientID %>').value;
            var userName = document.getElementById('<%= userName.ClientID %>').value;
            var userRole = document.getElementById('<%= userRole.ClientID %>').value;
            var contact = document.getElementById('<%= contact.ClientID %>').value;
            var status = document.getElementById('<%= status.ClientID %>').value;

            if (name.trim() === '' || userName.trim() === '' || userRole.trim() === '' || contact.trim() === '' || status.trim() === '') {
                validationAdd('All fields are required');
                return false; // Prevents the form submission
            }
        }

        function validationAdd(message) {
            $('#error_add').text(message); // Set the text of the error alert
            $('#error_add').removeClass('d-none').addClass('show');
            setTimeout(function () {
                $('#error_add').removeClass('show').addClass('d-none');
            }, 5000); // Hide alert after 5 seconds
        }

        function validateSearch() {
            var udName = document.getElementById('<%= udName.ClientID %>').value;

            if (udName.trim() === '') {
                validationUpdate('Please enter a name.');
                return false; // Prevents the form submission
            }
        }

        function validateUpdate() {
            var udName = document.getElementById('<%= udName.ClientID %>').value;
            var udUserName = document.getElementById('<%= udUserName.ClientID %>').value;
            var udUserRole = document.getElementById('<%= udUserRole.ClientID %>').value;
            var udStatus = document.getElementById('<%= udStatus.ClientID %>').value;

            if (udName.trim() === '' || udUserName.trim() === '' || udUserRole.trim() === '' || udStatus.trim() === '') {
                validationUpdate('All fields are required');
                return false; // Prevents the form submission
            }
        }

        function validationUpdate(message) {
            $('#error_update').text(message); // Set the text of the error alert
            $('#error_update').removeClass('d-none').addClass('show');
            setTimeout(function () {
                $('#error_update').removeClass('show').addClass('d-none');
            }, 5000); // Hide alert after 5 seconds
        }

        function showErrorAlert(message) {
            $('#errorAlert').text(message).removeClass('d-none').addClass('show');
            setTimeout(function () {
                $('#errorAlert').removeClass('show').addClass('d-none');
            }, 5000); // Hide alert after 5 seconds
        }

        function showSuccessAlert(message) {
            $('#successAlert').text(message).removeClass('d-none').addClass('show');
            setTimeout(function () {
                $('#successAlert').removeClass('show').addClass('d-none');
            }, 5000); // Hide alert after 5 seconds
        }

        function handleAccountFilterBtnClick() {
            // Perform AJAX request to trigger the server-side click event
            __doPostBack('<%= AccountFilterBtn.UniqueID %>', '');
        }

        document.getElementById('<%= AccountFilterBtn.ClientID %>').addEventListener('click', handleAccountFilterBtnClick);

    </script>


</asp:Content>
