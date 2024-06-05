﻿using DF2023.WebPageHelper;
using IdentityModel.Client;
using System;
using System.Web;
using System.Web.Security;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Security.Claims;
using System;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Security.Claims;
using Telerik.Sitefinity.Security.Model;
using System.Web;
using System.Linq;
using Telerik.Sitefinity.Services;
using System.Security.Claims;

namespace DF2023.WebPages
{
    public partial class DelegationPanel : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnGenerateDelegation_Click(object sender, EventArgs e)
        {
            string token = GetAuthenticatedUserAccessToken();
            var requestUrl = HttpContext.Current?.Request?.Url;
            string baseUrl = $"{requestUrl.Scheme}://{requestUrl.Host}:{requestUrl.Port}/";
            PanelHelper.Create8TousandsDelegation(baseUrl, token);
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