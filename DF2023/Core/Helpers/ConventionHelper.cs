using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace DF2023.Core.Helpers
{
    public static class ConventionHelper
    {
        public static bool isExistEmail(DynamicModuleManager dynamicManager, Guid id, string email)
        {
            DynamicContent convention = dynamicManager.GetDataItem(TypeResolutionService.ResolveType(Constants.ConventionType), id);
            if (convention != null && dynamicManager.HasChildItems(convention))
            {
                var delegations = dynamicManager.GetChildItems(convention, TypeResolutionService.ResolveType(Constants.DelegationType))
                    .Where(i => i.Status == ContentLifecycleStatus.Live && i.Visible)
                    .AsEnumerable()
                    .Where(i => i.GetValue("email")?.ToString() == "e");



                if (delegations !=null && delegations.Any())
                {

                }
            }
            return false;
        }
    }
}