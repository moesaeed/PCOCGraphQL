using DF2023.Core.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace DF2023.Core.Custom
{
    public class GuestManager : ContentHandler
    {
        public override bool IsDataValid(Dictionary<string, object> contextValue, out string errorMsg)
        {
            errorMsg = null;

            bool isGuestDuplicate = CheckGuestDuplicates(contextValue, out errorMsg);
            if (isGuestDuplicate)
            {
                return false;
            }

            return true;
        }

        public override void PostProcessData(DynamicContent item)
        {
        }

        public override void PreProcessData(Dictionary<string, object> contextValue)
        {
        }

        public static bool CheckGuestDuplicates(Dictionary<string, object> contextValue, out string errorMsg)
        {
            errorMsg = null;

            var id = contextValue.ContainsKey("id") ? Guid.Parse(contextValue["id"].ToString()) : Guid.Empty;
            var systemParentId = contextValue.ContainsKey("systemParentId") ? Guid.Parse(contextValue["systemParentId"].ToString()) : Guid.Empty;
            if (systemParentId == Guid.Empty)
            {
                errorMsg = "Can't create a guest without a parent";
                return true;
            }

            var email = contextValue.ContainsKey(Guest.Email.SetFirstLetterLowercase()) ? contextValue[Guest.Email.SetFirstLetterLowercase()].ToString() : string.Empty;
            var passportNumber = contextValue.ContainsKey(Guest.PassportNumber.SetFirstLetterLowercase()) ? contextValue[Guest.PassportNumber.SetFirstLetterLowercase()].ToString() : string.Empty;
            if (string.IsNullOrWhiteSpace(email))
            {
                errorMsg = "Email can't be null";
                return true;
            }

            var dynamicManager = DynamicModuleManager.GetManager();
            var type = TypeResolutionService.ResolveType(Guest.GuestDynamicTypeName);

            if (id == Guid.Empty)
            {
                DynamicContent convention = dynamicManager.GetDataItem(TypeResolutionService.ResolveType(Convention.ConventionDynamicTypeName), systemParentId);
                if (convention != null && dynamicManager.HasChildItems(convention))
                {
                    var guests = dynamicManager.GetChildItems(convention, type)
                        .Where(i => i.Status == ContentLifecycleStatus.Live && i.Visible
                        && i.PublishedTranslations.Any(pt =>
                                                       pt == SystemManager.CurrentContext.Culture.Name))
                        .FirstOrDefault(dc => dc.GetValue<string>(Guest.Email) == email || dc.GetValue<string>(Guest.PassportNumber) == passportNumber);

                    if (guests != null)
                    {
                        errorMsg = "There is a guest in this convention with the same email or passport number";
                        return true;
                    }
                }
            }
            else
            {
                var item = dynamicManager.GetDataItems(type).FirstOrDefault(i => i.Id == id);
                string currentEmail = item.GetValue<string>(Guest.Email);
                if (!string.IsNullOrWhiteSpace(currentEmail) && email.ToLower() != currentEmail.ToLower())
                {
                    errorMsg = "You can't change email address";
                    return true;
                }
            }

            return false;
        }
    }
}