using DF2023.Core.Constants;
using System;
using System.Globalization;
using Telerik.Sitefinity;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Localization;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Sitefinity.Workflow;

namespace DF2023.Core.Custom
{
    public class OTPManager
    {
        public static void GetUser(string email)
        {
            // Get user data from database using username
        }

        public static void UpdateUserAttempts(Guid userId, string email, int otpRequests, bool isLocked)
        {
            using (CultureRegion cr = new CultureRegion(new CultureInfo("en")))
            {
                var dynamicManager = DynamicModuleManager.GetManager();
                var type = TypeResolutionService.ResolveType(OTP.OTPDynamicTypeName);
                var item = dynamicManager.GetDataItem(type, userId);
                if (item == null)
                {
                    item = dynamicManager.CreateDataItem(type);
                }

                item.SetValue(OTP.Title, email);
                item.SetValue(OTP.UserID, userId);
                item.SetValue(OTP.OTPRequest, otpRequests);
                item.SetValue(OTP.IsLocked, isLocked);

                item.ApprovalWorkflowState = ApprovalStatusConstants.Published;
                ILifecycleDataItem publishedItem = dynamicManager.Lifecycle.Publish(item);
                item.SetWorkflowStatus(dynamicManager.Provider.ApplicationName, "Published");
                dynamicManager.SaveChanges();
            }
        }
    }
}