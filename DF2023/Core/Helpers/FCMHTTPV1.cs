using DF2023.Mvc.Models;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace DF2023.Core.Helpers
{
    public class FCMHTTPV1
    {
        public static bool SendPushNotification(PushNotificationViewModel pushNotification)
        {
            var manager = DynamicModuleManager.GetManager();
            var items = manager.GetDataItems(TypeResolutionService.ResolveType(Constants.DynamicModulesNames.FCMListType))
                .Where(x => x.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master)
                .ToList();
            if (items != null && items.Count == 1)
            {
                var item = items[0];
                var fcms = item.GetValue<string>("FCMUserList");
                if (!string.IsNullOrEmpty(fcms))
                {
                    List<string> stringList = fcms.Split(',').ToList();
                    if (stringList.Count > 0)
                    {
                        pushNotification.FcmToken = stringList;
                        var result = FCMHTTPV1.SendNotification(pushNotification);
                        return result.Result != 0;
                    }
                }
            }

            return false;
        }

        public static async Task<int> SendNotification(PushNotificationViewModel pushNotificationViewModel)
        {
            try
            {
                string fileName = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/FCM/calendar-2df0d-firebase-adminsdk-m0fby-d8947a9db2.json"); //Download from Firebase Console ServiceAccount
                if (File.Exists(fileName))
                {
                    // Read all text from the file
                    string fileContents = File.ReadAllText(fileName);
                }
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(fileName),
                    });
                }

                var message = new MulticastMessage()
                {
                    Notification = new Notification
                    {
                        Title = pushNotificationViewModel.Title,
                        Body = pushNotificationViewModel.Body,
                    },
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps()
                        {
                            Sound = "default"
                        }
                    },
                    Data = pushNotificationViewModel.Data,
                    Tokens = pushNotificationViewModel.FcmToken
                };
                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
                Log.Write(response.SuccessCount, ConfigurationPolicy.ABTestingTrace);
                Log.Write(response.Responses.ToString(), ConfigurationPolicy.ABTestingTrace);
                return response.SuccessCount;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                Log.Write(ex.Message, ConfigurationPolicy.ABTestingTrace);
                return 0;
            }
        }
    }
}