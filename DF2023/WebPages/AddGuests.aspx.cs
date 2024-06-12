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
                Conventions.DataSource = GetDataHelper.GetConventionsForDropDown(GetBaseUrl());
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
            string token = GetAuthenticatedUserAccessToken();
            var listguestCreated = PanelHelper.CreateGuest(GetBaseUrl(), NumberOfGuestToGenerate, Conventions.SelectedValue, token);
            grid.DataSource = listguestCreated.Results;
            grid.DataBind();

            if (listguestCreated.Errors != null && listguestCreated.Errors.Any())
            {
                labFailedResult.Text = labFailedResult.Text.Replace("[XXX]", $"[{listguestCreated.Errors.Count().ToString()}]");
                labFailedResult.Visible = true;
                FailedResult.Visible = true;
                FailedResult.DataSource = listguestCreated.Errors;
                FailedResult.DataBind();
            }
        }

        private string GetBaseUrl()
        {
            var requestUrl = HttpContext.Current?.Request?.Url;
            string baseUrl = $"{requestUrl.Scheme}://{requestUrl.Host}:{requestUrl.Port}/";
            return baseUrl;
        }
        public string GetAuthenticatedUserAccessToken()
        {
            var token = "Yjk3Mzg4OGEtMWRhYy00ZDU2LWJlMmMtZTZkZjc2NDJkMGZlLT1wcm92aWRlcj0tRGVmYXVsdC09c2VjcmV0a2V5PS1ZeT1YYm1UUV5LdTorbnRhQ3Z7PlB3SHR4bHRYJVFoTTtTbUpXYkA+cFBTRVMhVS1uUjRbU01NRUgvaFJOJlpYcFNuUXt2dEJ9dUZNXTtNYX10VmJZXl4yRCRKakxJOXdHRF9IQDBOd19JWlE9eU1qSCMoP0RiRl93QnBlbUttPQ==";
            return token;
        }
    }
}