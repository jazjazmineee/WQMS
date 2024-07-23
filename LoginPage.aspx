<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoginPage.aspx.cs" Inherits="WaterGuard_2024.LoginPage" %>

<!doctype html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatibile" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Login | Crystal Clear</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css" integrity="sha384-rbsA2VBKQhggwzxH7pPCaAqO46MgnOM80zW1RWuH61DGLwZJEdK2Kadq2F9CUG65" crossorigin="anonymous">
    <link rel="stylesheet" href="CSS/login_style.css">
</head>
<body>
    <form id="form1" runat="server">
        <!----------------------- Main Container -------------------------->
        <div class="container d-flex justify-content-center align-items-center min-vh-100">
            <!----------------------- Login Container -------------------------->
            <div class="row border rounded-5 p-3 bg-white shadow box-area">
                <div class="col-md-6 rounded-4 d-flex justify-content-center align-items-center flex-column left-box" style="background: #fff; border: solid 1px #cccccc;">
                    <div class="featured-image mb-3 d-none d-md-block">
                        <img src="images/CrystalClearLogo.png" class="img-fluid" style="width: 450px;">
                    </div>
                    <div class="d-md-none text-center mb-3">
                        <img src="images/CrystalClearLogo.png" class="img-fluid mt-3" style="width: 450px;">
                    </div>
                </div>

                <!-------------------------- Right Box ---------------------------->

                <div class="col-md-6 right-box">
                    <div class="row align-items-center">
                        <div class="header-text mb-4">
                            <h2>LOGIN</h2>
                            <p>Welcome back</p>
                        </div>

                        <div class="input-group mb-3">
                            <asp:TextBox ID="unametxtbox" runat="server" type="text" class="form-control form-control-lg bg-light fs-6" placeholder="Username"></asp:TextBox>
                        </div>
                        <div class="input-group mb-3">
                            <asp:TextBox ID="pwordtxtbox" runat="server" type="password" class="form-control form-control-lg bg-light fs-6" placeholder="Password"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <div class="alert alert-danger mb-2" id="error_alert" runat="server" Visible="false" role="alert">
                                <span id="errorMessage"></span>
                            </div>
                        </div>

                        <div class="input-group mt-2 mb-3 d-flex justify-content-between">

                            <div class="forgot">
                                <small><a href="ResetPass.aspx">Forgot Password?</a></small>
                            </div>
                        </div>
                        <div class="input-group mb-3">
                            <asp:Button ID="LoginBtn" runat="server" Text="Login" CssClass="btn btn-lg btn-primary w-100 fs-6" ValidationGroup="GroupLog" OnClick="LoginBtn_Click" />
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <script>
            function showAlertAndHide() {
                // Make the alert visible
                document.getElementById('error_alert').style.display = "block";

                // Set a timeout to hide the alert after 5 seconds
                setTimeout(function () {
                    // Hide the alert after 5 seconds
                    document.getElementById('error_alert').style.display = "none";
                }, 5000); // 5000 milliseconds = 5 seconds
            }
        </script>

        <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js" integrity="sha384-C6RzsynM9kWDrMNeT87bh95OGNyZPhcTNXj1NW7RuBCsyN/o0jlpcV8Qyq46cDfL" crossorigin="anonymous"></script>
        <script src="https://cdn.jsdelivr.net/npm/@popperjs/core@2.11.8/dist/umd/popper.min.js" integrity="sha384-I7E8VVD/ismYTF4hNIPjVp/Zjvgyol6VFvRkX/vR+Vc4jQkC+hVqc2pM8ODewa9r" crossorigin="anonymous"></script>
        <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.min.js" integrity="sha384-BBtl+eGJRgqQAUMxJ7pMwbEyER4l1g+O15P+16Ep7Q9Q+zqX6gSbd85u4mG4QzX+" crossorigin="anonymous"></script>
    </form>
</body>
</html>
