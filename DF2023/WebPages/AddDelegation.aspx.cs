using DF2023.WebPageHelper;
using System;
using System.Web;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Security.Claims;
using System.Linq;
using Telerik.Sitefinity.Services;
using System.Security.Claims;
using System.Web.UI.WebControls;
using System.Web.UI;
using DF2023.Core.Extensions;
using Telerik.OpenAccess.SPI;
using DF2023.WebPageModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GraphQL;
using ServiceStack;
using ServiceStack.Text;
using System.Collections.Generic;

namespace DF2023.WebPages
{
    public partial class AddDelegation : System.Web.UI.Page
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

        protected void btnGenerateDelegation_Click(object sender, EventArgs e)
        {
            int NumberOfDelegationToGenerate = 0;
            if (!string.IsNullOrWhiteSpace(NbrDelegation.Text))
                NumberOfDelegationToGenerate = Convert.ToInt32(NbrDelegation.Text);
            string token = GetAuthenticatedUserAccessToken();
            var listDelegationCreated = PanelHelper.CreateDelegation(GetBaseUrl(), NumberOfDelegationToGenerate, Guid.Parse(Conventions.SelectedValue), token);
            grid.DataSource = listDelegationCreated.Results;
            grid.DataBind();

            if(listDelegationCreated.Errors!=null && listDelegationCreated.Errors.Any())
            {
                labFailedResult.Text = labFailedResult.Text.Replace("[XXX]", $"[{listDelegationCreated.Errors.Count().ToString()}]");
                labFailedResult.Visible = true;
                FailedResult.Visible = true;
                FailedResult.DataSource = listDelegationCreated.Errors;
                FailedResult.DataBind();
            }
        }

        protected void grid_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewItem")
            {
                string itemId = e.CommandArgument.ToString();
                string url = $"../Sitefinity/adminapp/content/conventions/{Conventions.SelectedValue}/delegations/{itemId}/edit?sf_provider=OpenAccessProvider&sf_culture=en";
                ScriptManager.RegisterStartupScript(this, GetType(), "OpenItemInNewTab", $"window.open('{url}', '_blank')", true);
            }
            else if(e.CommandName == "ViewUser")
            {
                string itemId = e.CommandArgument.ToString();
                var user = UserExtensions.GetUserByEmail(itemId).Id;
                string url = $"../Sitefinity/Administration/Users";
                ScriptManager.RegisterStartupScript(this, GetType(), "OpenItemInNewTab", $"window.open('{url}', '_blank')", true);

            }
        }


        protected void btnGenerateGuests_Click(object sender, EventArgs e)
        {/*
            int NumberOfGuestToGenerate = 0;
            if (!string.IsNullOrWhiteSpace(NbrGuests.Text))
                NumberOfGuestToGenerate = Convert.ToInt32(NbrGuests.Text);
            string token = GetAuthenticatedUserAccessToken();
            PanelHelper.CreateGuest(GetBaseUrl(), NumberOfGuestToGenerate, Conventions.SelectedValue, token);*/
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
            // Check if the current user is authenticated
            if (SystemManager.CurrentHttpContext.User.Identity.IsAuthenticated)
            {
                // Get the claims principal
                var claimsPrincipal = SystemManager.CurrentHttpContext.User as ClaimsPrincipal;

                if (claimsPrincipal != null)
                {
                    // Retrieve the JWT token from the claims
                    var accessToken = claimsPrincipal.Claims
                        .FirstOrDefault(claim => claim.Type == "access_token")?.Value;

                    return accessToken;
                }
            }

            return null; // No authenticated user or access token not found
        }

        public string GetAuthenticatedUserAccessKey()
        {
            // Get the current authenticated user identity
            var identity = SystemManager.CurrentHttpContext.User.Identity as SitefinityIdentity;
            if (identity == null)
            {
                return null; // No authenticated user
            }

            // Get the UserManager for the specified membership provider
            var userManager = UserManager.GetManager(identity.MembershipProvider);

            // Get the user object
            var user = userManager.GetUser(identity.UserId);
            if (user == null)
            {
                return null; // User not found
            }

            // Retrieve the access key
            var accessKey = user.AccessKey;

            return accessKey.ToString();
        }

        private string GetAccessTokenForCurrentUser()
        {
            try
            {
                var principal = ClaimsManager.GetCurrentPrincipal();
                if (principal != null)
                {
                    var claim = principal.Claims.FirstOrDefault(c => c.Type == "access_token");
                    if (claim != null)
                    {
                        return claim.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them)
                return $"Error: {ex.Message}";
            }

            return null;
        }


    }
}