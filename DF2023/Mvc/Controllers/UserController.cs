using DF2023.Core;
using DF2023.Core.Constants;
using DF2023.Core.Custom;
using DF2023.Core.Extensions;
using DF2023.CutomAttributes;
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

        [Authorize]
        [HttpGet]
        public IHttpActionResult GetCurrentUserName()
        {
            ApiResult apiResult = null;
            try
            {
                string data = UserExtensions.GetCurentUserFirstLastName();

                apiResult = new ApiResult("Current User name", true, data);
            }
            catch (Exception ex)
            {
                apiResult = new ApiResult(ex.Message, false, null);
            }

            return this.Ok(apiResult);
        }

        [AuthorizeOTPAttribute]
        [HttpPost]
        public IHttpActionResult GenerateOTP(string userEmail)
        {
            if (string.IsNullOrWhiteSpace(userEmail) || userEmail.IsValidEmail() == false)
            {
                return this.Ok();
            }

            ApiResult apiResult = null;

            try
            {
                OTPManager oTPManager = new OTPManager();
                string result = oTPManager.GenerateOTP(userEmail);
                apiResult = new ApiResult("OTP", true, result);
            }
            catch (Exception ex)
            {
                apiResult = new ApiResult(ex.Message, false, null);
            }

            return this.Ok(apiResult);
        }

        [AuthorizeOTPAttribute]
        [HttpGet]
        public IHttpActionResult IsUserRegistered(string userEmail, string verificationCode)
        {
            ApiResult apiResult = null;

            try
            {
                apiResult = UserExtensions.IsUserRegistered(userEmail, verificationCode);
            }
            catch (Exception ex)
            {
                apiResult = new ApiResult(ex.Message, false, null);
            }

            return this.Ok(apiResult);
        }

        [AuthorizeOTPAttribute]
        [HttpPost]
        public IHttpActionResult ChangeUserPassword(string userEmail, string newPassword, string verificationCode, string otp)
        {
            ApiResult apiResult = null;

            try
            {
                apiResult = UserExtensions.ChangeUserPassword(userEmail, newPassword, verificationCode, otp);
            }
            catch (Exception ex)
            {
                apiResult = new ApiResult(ex.Message, false, null);
            }

            return this.Ok(apiResult);
        }
    }
}