using DF2023.WebPageHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DF2023.WebPages
{
    public partial class DelegationPanel : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnGenerateDelegation_Click(object sender, EventArgs e)
        {
            var requestUrl = HttpContext.Current?.Request?.Url;
            string baseUrl = $"{requestUrl.Scheme}://{requestUrl.Host}:{requestUrl.Port}/";
            PanelHelper.Create8TousandsDelegation(baseUrl);
        }
    }
}