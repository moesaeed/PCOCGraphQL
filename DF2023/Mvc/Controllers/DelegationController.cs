using DF2023.Core.Constants;
using DF2023.Core.Custom;
using DF2023.CutomAttributes;
using DF2023.Mvc.Models;
using System;
using System.Globalization;
using System.Web.Http;
using Telerik.Sitefinity.Services;

namespace DF2023.Mvc.Controllers
{
    [Authorize]
    public class DelegationController : ApiController
    {
        [HttpPost]
        [AuthorizeWithRoles(UserRoles.PCOC)]
        public IHttpActionResult SendInvitationEmail(Guid delegationID, Guid conventionID, string cultureName = "en")
        {
            ApiResult apiResult = null;
            try
            {
                SystemManager.CurrentContext.Culture = new CultureInfo(cultureName);

                bool result = DelegationEmailManager.SendInvitationEmail(delegationID, conventionID, out string errMsg);
                apiResult = new ApiResult(errMsg, result, null);
            }
            catch (Exception ex)
            {
                apiResult = new ApiResult(ex.Message, false, null);
            }

            return this.Ok(apiResult);
        }
    }
}