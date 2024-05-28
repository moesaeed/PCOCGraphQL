using DF2023.Core.Extensions;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace DF2023.CutomAttributes
{
    public class AuthorizeWithRolesAttribute : AuthorizeAttribute
    {
        private readonly string[] allowedroles;

        public AuthorizeWithRolesAttribute(params string[] roles)
        {
            this.allowedroles = roles;
        }

        public string ProviderName { get; set; }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            bool isAuthorized = base.IsAuthorized(actionContext);
            if (isAuthorized && allowedroles.Length > 0)
            {
                isAuthorized = false;
                foreach (var role in allowedroles)
                {
                    if (UserExtensions.IsCurrentUserInRole(role))
                    {
                        isAuthorized = true;
                        break;
                    }
                }
            }
            return isAuthorized;
        }
    }
}