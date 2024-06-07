using DF2023.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Security.Web.Services;

namespace DF2023.WebPages
{
    public partial class CPanel : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if(!isAdmin())
                HttpContext.Current.Response.Redirect("/");
        }

        private bool isAdmin()
        {
            var mgr = RoleManager.GetManager("AppRoles");
            var userId = SecurityManager.GetCurrentUserId();
            if (!mgr.IsUserInRole(userId, "Administrators"))
            {
                return false;
            }
            return true;
        }
    }
}