    <%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetPass.aspx.cs" Inherits="WaterGuard_2024.ResetPass" %>

<!doctype html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatibile" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Reset Password | WaterGuard</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css" integrity="sha384-rbsA2VBKQhggwzxH7pPCaAqO46MgnOM80zW1RWuH61DGLwZJEdK2Kadq2F9CUG65" crossorigin="anonymous">
    <link rel="stylesheet" href="CSS/login_style.css">
    <link href='https://unpkg.com/boxicons@2.1.4/css/boxicons.min.css' rel='stylesheet'>
    <link href="https://cdn.lineicons.com/4.0/lineicons.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <!----------------------- Main Container -------------------------->

        <div class="container d-flex justify-content-center align-items-center min-vh-100">
            <!----------------------- Reset Container -------------------------->
            <div class="row border rounded-5 p-3 bg-white shadow box-area">
                <div class="col-md-6 rounded-4 d-flex justify-content-center align-items-center flex-column left-box" style="background: #fff; border: solid 1px #cccccc;">

                    <div class="featured-image mb-3 d-none d-md-block">
                        <img src="images/CrystalClearLogo.png" class="img-fluid" style="width: 450px;">
                    </div>

                    <div class="d-md-none text-center mb-3">
                        <img src="images/CrystalClearLogo.png" class="img-fluid mt-3" style="width: 450px;">
                    </div>

                </div>

                <!-------------------- ------ Right Box ---------------------------->

                <div class="col-md-6 right-box">
                    <div class="header-text mb-2">
                        <!-- Back button -->
                        <a class="btn fs-3" href="LoginPage.aspx">
                            <i class="lni lni-arrow-left"></i>
                        </a>
                        <!-- End of back button -->
                    </div>

                    <div class="row align-items-center">
                        <div class="header-text mb-4">
                            <h2>RESET PASSWORD</h2>
                        </div>
                        <div class="input-group mb-3">
                            <asp:TextBox ID="uname" runat="server" type="text" class="form-control form-control-lg bg-light fs-6" placeholder="Username"></asp:TextBox>
                        </div>
                        <div class="input-group mb-3">
                            <asp:TextBox ID="newPassword" runat="server" type="password" class="form-control form-control-lg bg-light fs-6" placeholder="New Password"></asp:TextBox>
                        </div>
                        <div class="input-group mb-3">
                            <asp:TextBox ID="confirmPassword" runat="server" type="password" class="form-control form-control-lg bg-light fs-6" placeholder="Confirm Password"></asp:TextBox>
                        </div>

                        <div class="input-group mb-3">
                            <asp:TextBox ID="otp" runat="server" type="text" class="form-control form-control-lg bg-light fs-6" placeholder="OTP" ValidationGroup="GroupRes"></asp:TextBox>
                            <asp:Button ID="sendOTPBtn" runat="server" class="btn btn-primary" Text="Send OTP" OnClick="sendOTPBtn_Click" ValidationGroup="GroupOTP" BackColor="#0D6EFD" ForeColor="White" />
                        </div>
                        <div class="input-group mb-3">
                            <asp:HiddenField ID="hdnRemainingTime" runat="server" />
                            <asp:Label ID="countdownLabel" runat="server" Text=""></asp:Label>
                        </div>
                        <div class="mb-3">
                            <div class="alert alert-danger mb-2" id="error_alert" runat="server" visible="false" role="alert">
                                <span id="errorMessage"></span>
                            </div>
                        </div>
                        <div class="mb-3">
                            <div class="alert alert-success mb-2" id="success_alert" runat="server" visible="false" role="alert">
                                <span id="successMessage"></span>
                            </div>
                        </div>
                    </div>
                    <div class="input-group mb-3">
                        <asp:Button ID="resetPasswordSubmitBtn" runat="server" class="btn btn-lg btn-primary w-100 fs-6" Text="Reset Password" OnClick="resetPasswordSubmitBtn_Click" ValidationGroup="GroupRes" />
                    </div>
                </div>

            </div>
        </div>

        <script>
            // JavaScript function to start the countdown timer
            function startCountdown() {
                var countdownLabel = document.getElementById('<%= countdownLabel.ClientID %>');
                var remainingTimeValue = document.getElementById('<%= hdnRemainingTime.ClientID %>').value;

                if (remainingTimeValue) {
                    var remainingTime = parseInt(remainingTimeValue); // Get remaining time from hidden field

                    // Set the initial countdown label text dynamically
                    var minutes = Math.floor(remainingTime / 60);
                    var seconds = remainingTime % 60;
                    countdownLabel.textContent = "Resend OTP in: " + ("0" + minutes).slice(-2) + "m " + ("0" + seconds).slice(-2) + "s";

                    var countdownInterval = setInterval(function () {
                        if (remainingTime <= 0) {
                            clearInterval(countdownInterval); // Stop the countdown
                            sendOTPBtn.disabled = false; // Enable the "Resend OTP" button
                            countdownLabel.textContent = ""; // Clear the countdown text
                        } else {
                            var minutes = Math.floor(remainingTime / 60);
                            var seconds = remainingTime % 60;
                            countdownLabel.textContent = "Resend OTP in: " + ("0" + minutes).slice(-2) + "m " + ("0" + seconds).slice(-2) + "s";
                            remainingTime--; // Decrement remaining time
                            document.getElementById('<%= hdnRemainingTime.ClientID %>').value = remainingTime; // Update remaining time in hidden field
                        }
                    }, 1000); // Update every second
                } else {
                    // Handle case where hidden field value is not available
                    console.error("Hidden field value is not available.");
                }
            }


            function clearCountdownInterval() {
                clearInterval(window.countdownInterval);
            }

            function showAlertAndHide(alertId) {
                // Make the alert visible
                document.getElementById(alertId).style.display = "block";

                // Set a timeout to hide the alert after 5 seconds
                setTimeout(function () {
                    // Hide the alert after 5 seconds
                    document.getElementById(alertId).style.display = "none";
                }, 5000); // 5000 milliseconds = 5 seconds
            }
        </script>

        <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js" integrity="sha384-C6RzsynM9kWDrMNeT87bh95OGNyZPhcTNXj1NW7RuBCsyN/o0jlpcV8Qyq46cDfL" crossorigin="anonymous"></script>
        <script src="https://cdn.jsdelivr.net/npm/@popperjs/core@2.11.8/dist/umd/popper.min.js" integrity="sha384-I7E8VVD/ismYTF4hNIPjVp/Zjvgyol6VFvRkX/vR+Vc4jQkC+hVqc2pM8ODewa9r" crossorigin="anonymous"></script>
        <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.min.js" integrity="sha384-BBtl+eGJRgqQAUMxJ7pMwbEyER4l1g+O15P+16Ep7Q9Q+zqX6gSbd85u4mG4QzX+" crossorigin="anonymous"></script>


    </form>
</body>
</html>
