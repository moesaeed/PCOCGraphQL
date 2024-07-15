using DF2023.Core.Constants;
using DF2023.Core.Extensions;
using DF2023.Mvc.Models;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Localization;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Sitefinity.Versioning;
using Telerik.Sitefinity.Workflow;

namespace DF2023.Core.Custom
{
    public class OTPManager
    {
        private OTPDTO GetUser(Guid userId)
        {
            using (CultureRegion cr = new CultureRegion(new CultureInfo("en")))
            {
                var dynamicManager = DynamicModuleManager.GetManager();
                using (ElevatedModeRegion elevatedUserRegion = new ElevatedModeRegion(dynamicManager))
                {
                    var type = TypeResolutionService.ResolveType(OTP.OTPDynamicTypeName);
                    var item = dynamicManager.GetDataItems(type).FirstOrDefault(x => x.Id == userId);
                    if (item == null)
                    {
                        return null;
                    }

                    OTPDTO oTPDTO = new OTPDTO();
                    if (item != null)
                    {
                        oTPDTO.Email = item.GetValue<string>(OTP.Title);

                        var value = item.GetValue<decimal?>(OTP.OTPRequest);
                        oTPDTO.OTPRequests = (int)value.GetValueOrDefault(0);

                        oTPDTO.IsLocked = item.GetValue<bool>(OTP.IsLocked);
                        oTPDTO.OTPCode = item.GetValue<string>(OTP.OTPCode);
                        oTPDTO.UserID = item.Id;
                        return oTPDTO;
                    }
                }
            }

            return null;
        }

        private void UpdateUserAttempts(OTPDTO oTPDTO)
        {
            using (CultureRegion cr = new CultureRegion(new CultureInfo("en")))
            {
                var dynamicManager = DynamicModuleManager.GetManager();
                using (ElevatedModeRegion elevatedUserRegion = new ElevatedModeRegion(dynamicManager))
                {
                    var type = TypeResolutionService.ResolveType(OTP.OTPDynamicTypeName);
                    var item = dynamicManager.GetDataItems(type).FirstOrDefault(i => i.Id == oTPDTO.UserID);
                    if (item == null)
                    {
                        item = dynamicManager.CreateDataItem(type, oTPDTO.UserID, "/DynamicModule");
                    }

                    item.SetValue(OTP.Title, oTPDTO.Email);
                    item.SetValue(OTP.UserID, oTPDTO.UserID.ToString());
                    item.SetValue(OTP.OTPRequest, oTPDTO.OTPRequests);
                    item.SetValue(OTP.IsLocked, oTPDTO.IsLocked);
                    item.SetValue(OTP.OTPCode, oTPDTO.OTPCode);

                    item.ApprovalWorkflowState = ApprovalStatusConstants.Published;
                    ILifecycleDataItem publishedItem = dynamicManager.Lifecycle.Publish(item);
                    item.SetWorkflowStatus(dynamicManager.Provider.ApplicationName, "Published");
                    dynamicManager.SaveChanges();

                    var versionManager = VersionManager.GetManager();
                    var version = versionManager.CreateVersion(item, true);
                    versionManager.SaveChanges();
                }
            }
        }

        public string GenerateOTP(string userEmail)
        {
            // 1. Retrieve user information
            var roles = new List<string>()
            {
                UserRoles.GuestAdmin,
                UserRoles.PCOC
            };

            Guid userId = UserExtensions.IsUserByEmailInRoles(roles, userEmail);
            if (userId == Guid.Empty)
            {
                if (userId == Guid.Empty)
                {
                    return null;
                }
            }

            OTPDTO oTPDTO = GetUser(userId);

            // 2. Handle non-existent user
            if (oTPDTO == null)
            {
                oTPDTO = CreateNewUser(userEmail, userId);
            }

            // 3. Validate user and track attempts
            if (!ValidateUser(oTPDTO))
            {
                return null;
            }

            // 4. Generate and return OTP
            string otpCode = GenerateOtpCode();
            oTPDTO.OTPCode = otpCode;
            UpdateUserAttempts(oTPDTO);
            return otpCode;
        }

        private OTPDTO CreateNewUser(string userEmail, Guid userId)
        {
            var oTPDTO = new OTPDTO()
            {
                Email = userEmail,
                UserID = userId,
            };
            return oTPDTO;
        }

        private bool ValidateUser(OTPDTO oTPDTO)
        {
            if (oTPDTO.IsLocked)
            {
                return false;
            }

            const int maxAttempts = 5;

            if (oTPDTO.OTPRequests < maxAttempts)
            {
                oTPDTO.OTPRequests++;
                return true;
            }

            oTPDTO.IsLocked = true;
            UpdateUserAttempts(oTPDTO);
            return false;
        }

        public string GenerateOtpCode()
        {
            var key = KeyGeneration.GenerateRandomKey(OtpHashMode.Sha512);
            var totp = new Totp(key, mode: OtpHashMode.Sha512, step: 60);
            return totp.ComputeTotp();
        }
    }
}