using DF2023.Core.Configs;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Configuration;

namespace DF2023.CutomAttributes
{
    public class AuthorizeOTPAttribute : AuthorizationFilterAttribute
    {
        public AuthorizeOTPAttribute()
        { }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Contains("Authorization"))
            {
                var authHeader = actionContext.Request.Headers.GetValues("Authorization").FirstOrDefault();
                if (string.IsNullOrWhiteSpace(authHeader) == false && authHeader.StartsWith("Basic "))
                {
                    try
                    {
                        var config = Config.Get<OTPConfig>();
                        var expectedAuthHeader = config.EndpointHeaderValue;
                        if (string.IsNullOrWhiteSpace(expectedAuthHeader))
                        {
                            ForceReturnBadRequest(actionContext);
                            return;
                        }

                        string encodedPDK = authHeader.Substring("Basic ".Length).Trim();
                        string pDK = Encoding.ASCII.GetString(Convert.FromBase64String(encodedPDK));
                        if (pDK == expectedAuthHeader)
                        {
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex, ConfigurationPolicy.ErrorLog);
                    }
                }
            }

            ForceReturnBadRequest(actionContext);
        }

        private static void ForceReturnBadRequest(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Forbidden");
        }
    }
}