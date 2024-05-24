using DF2023.Core;
using DF2023.Mvc.Models;
using System;
using System.Globalization;
using System.Web.Http;
using Telerik.Sitefinity.Services;

namespace DF2023.Mvc.Controllers
{
    public class UserController : ApiController
    {
        [HttpPost]
        public IHttpActionResult AddFcmToken(string fcmToken, string cultureName = "en")
        {
            ApiResult apiResult = null;
            try
            {
                SystemManager.CurrentContext.Culture = new CultureInfo(cultureName);

                apiResult = new ApiResult("Add FCM Token", true, FCMHelper.AddFCM(fcmToken));
            }
            catch (Exception ex)
            {
                apiResult = new ApiResult(ex.Message, false, null);
            }

            return this.Ok(apiResult);
        }
    }
}