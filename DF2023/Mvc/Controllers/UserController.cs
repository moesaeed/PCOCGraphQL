using DF2023.Core;
using DF2023.Core.Constants;
using DF2023.Core.Extensions;
using DF2023.CutomAttributes;
using DF2023.Mvc.Models;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        [AuthorizeWithRoles(UserRoles.GuestAdmin)]
        [HttpGet]
        public HttpResponseMessage GetGuestConventionDetails()
        {
            ApiResult apiResult = null;
            try
            {
                string data = UserExtensions.GetUserCustomfieldValue(Others.UserCustomField);

                var contentString = new StringContent(data);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = contentString;
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                return response;
            }
            catch (Exception ex)
            {
            }

            return null;
        }
    }
}