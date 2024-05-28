using System.Collections.Generic;
using System.Linq;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace DF2023.Core
{
    public class FCMHelper
    {
        public static bool AddFCM(string fcmToken)
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
                    if (!stringList.Contains(fcmToken))
                    {
                        stringList.Add(fcmToken);
                        string newCommaSeparatedString = string.Join(",", stringList);

                        item.SetValue("FCMUserList", newCommaSeparatedString);
                        manager.SaveChanges();

                        return true;
                    }
                }
                else
                {
                    List<string> stringList = new List<string>();
                    stringList.Add(fcmToken);
                    string newCommaSeparatedString = string.Join(",", stringList);

                    item.SetValue("FCMUserList", newCommaSeparatedString);
                    ILifecycleDataItem publishedFcmlistItem = manager.Lifecycle.Publish(item);
                    manager.SaveChanges();

                    return true;
                }
            }

            return false;
        }
    }
}