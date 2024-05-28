using DF2023.Mvc.Models;
using System;
using System.Collections.Generic;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace DF2023.Core.Helpers
{
    public class NotificationHelper
    {
        public static bool SendNotification(DynamicContent item)
        {
            try
            {
                DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager();
                Type notificationType = TypeResolutionService.ResolveType(Constants.DynamicModulesNames.NotificationType);
                DynamicContent notificationItem = dynamicModuleManager.GetDataItem(notificationType, item.Id);
                if (notificationItem != null && notificationItem.GetValue<bool>("SendNotification") == true)
                {
                    var pushNotification = new PushNotificationViewModel();
                    pushNotification.Title = notificationItem.GetValue("Title").ToString();
                    pushNotification.Body = notificationItem.GetValue("Description").ToString();
                    Dictionary<string, string> data = new Dictionary<string, string>();
                    data.Add("UpdateData", notificationItem.GetValue<bool>("UpdateData").ToString());
                    pushNotification.Data = data;
                    FCMHTTPV1.SendPushNotification(pushNotification);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, ConfigurationPolicy.ErrorLog);
                Log.Write(ex, ConfigurationPolicy.ABTestingTrace);
            }

            return false;
        }
    }
}