<%@ Page Title="" Language="C#" MasterPageFile="~/Sidebar.Master" AutoEventWireup="true" CodeBehind="UserProfile.aspx.cs" Inherits="WaterGuard_2024.UserProfile" %>

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
            <h1 class="h1 mb-0 text-gray-800">Profile</h1>
        </div>


        <div class="row">
            <div class="col-lg-12 mb-4">
                <div class="card">
                    <div class="card-header">
                        <h5>Account Information</h5>
                    </div>
                    <div class="row g-0">
                        <div class="col-12 col-sm-12">
                            <div class="card-body">
                                <div class="mb-2">
                                    <%--Name--%>
                                    <label class="form-label">Name</label>
                                    <asp:TextBox ID="nameTxtbox" runat="server" CssClass="form-control" Enabled="false" Style="background: white;">

                                    </asp:TextBox>
                                </div>
                                <div class="mb-2">
                                    <%--Username--%>
                                    <label class="form-label">Username</label>
                                    <asp:TextBox ID="usernameTxtbox" runat="server" CssClass="form-control" Enabled="false" Style="background: white;" placeholder="Username"></asp:TextBox>
                                </div>
                                <div class="mb-3">
                                    <%--Contact--%>
                                    <label class="form-label">Contact</label>
                                    <asp:TextBox ID="contactTxtbox" runat="server" CssClass="form-control" Enabled="false" Style="background: white;" placeholder="Contact"></asp:TextBox>
                                </div>
                                <div class="mb-2">
                                    <%--Buttons--%>
                                    <asp:Button CssClass="btn btn-primary" ID="Button3" runat="server" Text="Update Profile" OnClientClick="openEditProfileModal(); return false;" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-lg-12 mb-4">
                <div class="card">
                    <div class="card-header">
                        <h5>Account Information</h5>
                    </div>
                    <div class="row g-0">
                        <div class="col-12 col-sm-12">
                            <div class="card-body">
                                <div class="mb-2">
                                    <%--Buttons--%>
                                    <asp:Button CssClass="btn btn-warning" ID="Button2" runat="server" Text="Update Password" OnClientClick="openEditPasswordModal(); return false;" />
                                </div>
                                <div class="alert alert-danger d-none mb-2" id="errorAlert" role="alert">
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <!-- Edit Pass Modal -->
        <div class="modal fade" id="editPassModal" tabindex="-1" aria-labelledby="editPassModalLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="editPassModalLabel">Edit Profile</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close" onclick="clearFields()"></button>
                    </div>
                    <div class="modal-body">
                        <div class="mb-3">
                            <label for="currPassword" class="form-label">Current Password</label>
                            <asp:TextBox ID="currPassword" runat="server" type="password" class="form-control" name="currPassword"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label for="newPassword" class="form-label">New Password</label>
                            <asp:TextBox ID="newPassword" runat="server" type="password" class="form-control" name="newPassword"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label for="confirmPassword" class="form-label">Confirm Password</label>
                            <asp:TextBox ID="confirmPassword" runat="server" type="password" class="form-control" name="confirmPassword"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <div class="alert alert-danger d-none mb-2" id="error_alert" role="alert"></div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <asp:Button ID="udPassBtn" runat="server" class="btn btn-primary" Text="Save Changes" OnClientClick="return validateBeforeSubmit();" OnClick="udPassBtn_Click" />
                    </div>
                </div>
            </div>
        </div>


        <!-- Edit Profile Modal -->
        <div class="modal fade" id="editModal" tabindex="-1" aria-labelledby="editModallLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header text-">
                        <h5 class="modal-title" id="editModalLabel">Edit Profile</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close" onclick="clearFields()"></button>
                    </div>
                    <div class="modal-body">
                        <div class="mb-3">
                            <div class="input-group">
                                <span class="input-group-text">Name</span>
                                <asp:TextBox ID="udName" runat="server" CssClass="form-control" Style="background: white;"></asp:TextBox>
                            </div>
                        </div>
                        <div class="mb-3">
                            <div class="input-group">
                                <span class="input-group-text">Username</span>
                                <asp:TextBox ID="udUsername" runat="server" CssClass="form-control" Style="background: white;"></asp:TextBox>
                            </div>
                        </div>
                        <div class="mb-3">
                            <div class="input-group">
                                <span class="input-group-text">Contact</span>
                                <asp:TextBox ID="udContact" runat="server" CssClass="form-control" Style="background: white;"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <asp:Button ID="saveChangesBtn" runat="server" class="btn btn-primary" data-bs-dismiss="modal" Text="Save Changes" OnClick="saveChangesBtn_Click" />

                    </div>
                </div>
            </div>
        </div>

    </div>

    <script>
        function openEditPasswordModal() {
            $('#editPassModal').modal('show');
        }

        function openEditProfileModal() {
            $('#editModal').modal('show');
        }

        function validateBeforeSubmit() {
            var currPassword = document.getElementById('<%= currPassword.ClientID %>').value;
            var newPassword = document.getElementById('<%= newPassword.ClientID %>').value;
            var confirmPassword = document.getElementById('<%= confirmPassword.ClientID %>').value;

            if (currPassword.trim() === '' || newPassword.trim() === '' || confirmPassword.trim() === '') {
                validationAlert('All fields are required');
                return false; // Prevents the form submission
            }
            else if (newPassword !== confirmPassword) {
                validationAlert('Passwords do not match.');
                return false; // Prevents the form submission
            }

            return true; // Allow form submission if validation passes
        }


        function showErrorAlert(message) {
            $('#errorAlert').text(message).removeClass('d-none').addClass('show');
            setTimeout(function () {
                $('#errorAlert').removeClass('show').addClass('d-none');
            }, 5000); // Hide alert after 5 seconds
        }

        function validationAlert(message) {
            $('#error_alert').text(message); // Set the text of the error alert
            $('#error_alert').removeClass('d-none').addClass('show');
            setTimeout(function () {
                $('#error_alert').removeClass('show').addClass('d-none');
            }, 5000); // Hide alert after 5 seconds
        }

        function clearFields() {
            document.getElementById('<%= currPassword.ClientID %>').value = '';
            document.getElementById('<%= newPassword.ClientID %>').value = '';
            document.getElementById('<%= confirmPassword.ClientID %>').value = '';
            document.getElementById('<%= udContact.ClientID %>').value = '';
            document.getElementById('<%= udName.ClientID %>').value = '';
            document.getElementById('<%= udUsername.ClientID %>').value = '';
        }
    </script>

</asp:Content>
