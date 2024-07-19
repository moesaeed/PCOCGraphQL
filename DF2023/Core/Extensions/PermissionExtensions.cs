using System;
using System.Collections.Generic;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Model;

namespace DF2023.Core.Extensions
{
    public static class PermissionExtensions
    {
        public static void SetPermission(DynamicContent item, List<Guid> userIds, List<string> roles)
        {
            if (userIds != null)
            {
                foreach (var userId in userIds)
                {
                    item.ManagePermissions()
                         .ForUser(userId)
                         .Grant().View()
                         .Grant().Modify()
                         .Grant().Create()
                         .Grant().Delete();
                }
            }
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    item.ManagePermissions()
                        .ForRole(role)
                        .Grant().View()
                        .Grant().Modify()
                        .Grant().Create()
                        .Grant().Delete();
                }
            }
        }

        public static void ClearPermission(DynamicContent item)
        {
            item.ManagePermissions().ClearAll();
        }
    }
}