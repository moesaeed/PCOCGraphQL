using System.Web;
using Telerik.Sitefinity.Security.Data;
using Telerik.Sitefinity.Security.Model;

namespace DF2023
{
    public class CustomMembershipProvider : OpenAccessMembershipProvider
    {
        protected override void UpdateFailureCount(User user, string failureType)
        {
            base.UpdateFailureCount(user, failureType);
            if (HttpContext.Current.Request.Url.LocalPath.Contains("sitefinity/oauth/token") && failureType == "password") //only run this logic when requests to this endpoint are made and the failure type is because of incorrect password.
            {
                var provider = ((Telerik.Sitefinity.Model.IDataItem)user).Provider as OpenAccessMembershipProvider;
                provider.SuppressSecurityChecks = true; //under the current context, there is no user being authenticated. We need to suppress the checks so that we can commit changes to the DB.

                provider.CommitTransaction();

                provider.SuppressSecurityChecks = false; //reestablishing the security checks on provider level after the change is committed.
            }
        }
    }
}