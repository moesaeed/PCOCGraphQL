using DF2023.WebPageHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Sitefinity.Services;

namespace DF2023.WebPages
{
    public partial class AddGuests : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Conventions.DataSource = GetDataHelper.GetConventionsForDropDown(CommonHelper.GetBaseUrl());
                Conventions.DataTextField = "Title";
                Conventions.DataValueField = "Id";
                Conventions.DataBind();
            }
        }


        protected void btnGenerateGuest_Click(object sender, EventArgs e)
        {
            int NumberOfGuestToGenerate = 0;
            if (!string.IsNullOrWhiteSpace(NbrGuest.Text))
                NumberOfGuestToGenerate = Convert.ToInt32(NbrGuest.Text);
            string token = CommonHelper.GetAuthenticatedUserAccessToken();
            var listguestCreated = PanelHelper.CreateGuest(CommonHelper.GetBaseUrl(), NumberOfGuestToGenerate,Guid.Parse(Conventions.SelectedValue), token);
            if (listguestCreated.Results.Any())
            {
                grid.DataSource = listguestCreated.Results;
                grid.DataBind();
            }
            if (listguestCreated.Errors != null && listguestCreated.Errors.Any())
            {
                labFailedResult.Text = labFailedResult.Text.Replace("[XXX]", $"[{listguestCreated.Errors.Count().ToString()}]");
                labFailedResult.Visible = true;
                FailedResult.Visible = true;
                FailedResult.DataSource = listguestCreated.Errors;
                FailedResult.DataBind();
            }
        }

        protected void grid_RowCommand(object sender, GridViewCommandEventArgs e)
        {

        }
    }
}