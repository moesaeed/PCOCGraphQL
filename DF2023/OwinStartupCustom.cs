using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin;
using Owin;
using System;
using Telerik.Sitefinity.Owin;
using Microsoft.Owin.Security.Infrastructure;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace SitefinityWebApp
{
    public class OwinStartupCustom
    {
        private static readonly Type refreshTokenProviderTypeInfo = TypeResolutionService.ResolveType("Telerik.Sitefinity.Authentication.OAuth.SitefinityOAuthRefreshTokenProvider");
        private static readonly Type authorizationTokenProviderTypeInfo = TypeResolutionService.ResolveType("Telerik.Sitefinity.Authentication.OAuth.SitefinityOAuthAuthorizationCodeProvider");

        public void Configuration(IAppBuilder app)
        {
            // Register default Sitefinity middlewares in the pipeline
            app.UseSitefinityMiddleware();

            // Add custom OAuth route
            var defaultRefreshTokenProviderInstance = Activator.CreateInstance(refreshTokenProviderTypeInfo) as AuthenticationTokenProvider;
            var defaultAuthorizationTokenProviderInstance = Activator.CreateInstance(authorizationTokenProviderTypeInfo) as AuthenticationTokenProvider;
            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AuthorizeEndpointPath = new PathString("/oauth/authorize-custom"),
                TokenEndpointPath = new PathString("/oauth/token-custom"),
                AccessTokenExpireTimeSpan = TimeSpan.FromSeconds(7200),
                Provider = new SitefinityOAuthServerProviderCustom(),
                AllowInsecureHttp = true,
                RefreshTokenProvider = defaultRefreshTokenProviderInstance,
                AuthorizationCodeProvider = defaultAuthorizationTokenProviderInstance
            });
        }
    }
}