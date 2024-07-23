using System;

public class BasePage : System.Web.UI.Page
{
    protected override void OnPreInit(EventArgs e)
    {
        string userRole = GetUserRole();

        if (userRole.Equals("Manager", StringComparison.OrdinalIgnoreCase))
        {
            this.MasterPageFile = "~/Sidebar2.master";
        }
        else if (userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase))
        {
            this.MasterPageFile = "~/Sidebar2.master";
        }
        else if (userRole.Equals("Owner", StringComparison.OrdinalIgnoreCase))
        {
            this.MasterPageFile = "~/Sidebar.master";
        }

        base.OnPreInit(e);
    }

    private string GetUserRole()
    {
        return Session["UserRole"] as string ?? "default";
    }
}
