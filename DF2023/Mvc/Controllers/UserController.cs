using DF2023.Core;
using DF2023.Core.Constants;
using DF2023.Core.Extensions;
using DF2023.CutomAttributes;
using DF2023.Mvc.Models;
using OtpNet;
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

        [AuthorizeWithRoles(UserRoles.GuestAdmin)]
        [HttpGet]
        public IHttpActionResult GetGuestConventionDetails()
        {
            ApiResult apiResult = null;
            try
            {
                string data = UserExtensions.GetUserCustomfieldValue(Others.UserCustomField, UserExtensions.GetCurentUserId());

                apiResult = new ApiResult("Guest Convention Details", true, data);
            }
            catch (Exception ex)
            {
                apiResult = new ApiResult(ex.Message, false, null);
            }

            return this.Ok(apiResult);
        }

        [AuthorizeOTPAttribute]
        [HttpPost]
        public IHttpActionResult GenerateOPT(string userEmail)
        {
            if (string.IsNullOrWhiteSpace(userEmail) || userEmail.IsValidEmail() == false)
            {
                return this.Ok();
            }

            ApiResult apiResult = null;

            try
            {
                Guid isUserByEmailInRole = UserExtensions.IsUserByEmailInRole(UserRoles.GuestAdmin, userEmail);
                if (isUserByEmailInRole == Guid.Empty)
                {
                    return this.Ok();
                }
                
                var key = KeyGeneration.GenerateRandomKey(OtpHashMode.Sha512);
                var totp = new Totp(key, mode: OtpHashMode.Sha512, step: 60);
                var result = totp.ComputeTotp();

                apiResult = new ApiResult("OTP", true, result);
            }
            catch (Exception ex)
            {
                apiResult = new ApiResult(ex.Message, false, null);
            }

            return this.Ok(apiResult);
        }
    }
}