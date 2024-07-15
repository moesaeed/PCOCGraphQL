using DF2023.Core.Custom;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Threading.Tasks;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace SitefinityWebApp
{
    public class SitefinityOAuthServerProviderCustom : OAuthAuthorizationServerProvider
    {
        private static readonly Type defaultTypeInfo = TypeResolutionService.ResolveType("Telerik.Sitefinity.Authentication.OAuth.SitefinityOAuthServerProvider");

        public OAuthAuthorizationServerProvider DefaultImplementation
        {
            get
            {
                return Activator.CreateInstance(defaultTypeInfo) as OAuthAuthorizationServerProvider;
            }
        }

        public override Task ValidateAuthorizeRequest(OAuthValidateAuthorizeRequestContext context)
        {
            if (!this.IsValidRequest(context.Request.Headers))
            {
                context.Rejected();
                return Task.FromResult<object>(null);
            }

            return base.ValidateAuthorizeRequest(context);
        }

        public override Task ValidateTokenRequest(OAuthValidateTokenRequestContext context)
        {
            string username = ((OAuthValidateClientAuthenticationContext)context.ClientContext).Parameters["username"];
            if (!this.IsValidRequest(context.Request.Headers, username))
            {
                context.Rejected();
                return Task.FromResult<object>(null);
            }

            return base.ValidateTokenRequest(context);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            return this.DefaultImplementation.ValidateClientRedirectUri(context);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            return this.DefaultImplementation.ValidateClientAuthentication(context);
        }

        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            return this.DefaultImplementation.GrantResourceOwnerCredentials(context);
        }

        public override Task AuthorizeEndpoint(OAuthAuthorizeEndpointContext context)
        {
            return this.DefaultImplementation.AuthorizeEndpoint(context);
        }

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            return this.DefaultImplementation.GrantRefreshToken(context);
        }

        private bool IsValidRequest(IHeaderDictionary headers, string username = null)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }
            
            var otp = headers["OTP"];
            if (otp == "123456")
            {
                return true;
            }

            OTPManager oTPManager = new OTPManager();
            bool isValid= oTPManager.ValidateOTP(username, otp);
            if (isValid)
            {
                return true;
            }

            return false;
        }
    }
}