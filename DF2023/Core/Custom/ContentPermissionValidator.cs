using DF2023.Core.Constants;
using DF2023.Core.Extensions;
using System.Collections.Generic;

namespace DF2023.Core.Custom
{
    public static class ContentPermissionValidator
    {
        private static List<string> GuestReleatedContentTypies = new List<string>() {
            Country.CountryDynamicTypeName,
            Passporttype.PassporttypeDynamicTypeName,
            Titlelist.TitlelistDynamicTypeName,
            Airport.AirportDynamicTypeName,
            Gueststatus.GueststatusDynamicTypeName,
            Delegationmembertype.DelegationmembertypeDynamicTypeName,
            ServicesLevel.ServicesLevelDynamicTypeName,
        };

        // If Guest Admin user, then we skip creating releated item, and get the item by ID
        // As Guest admin is not allowed to create related data
        
        public static bool SkipCreatingContent(string contentType)
        {
            bool isGuestAdmin = UserExtensions.IsCurrentUserInRole(UserRoles.GuestAdmin);
            if (isGuestAdmin
                && !string.IsNullOrWhiteSpace(contentType)
                && GuestReleatedContentTypies.Contains(contentType))
            {
                return true;
            }

            return false;
        }
    }
}