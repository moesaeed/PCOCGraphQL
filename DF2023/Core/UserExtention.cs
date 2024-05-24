using System;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Model.ContentLinks;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Security.Model;

namespace DF2023.Core
{
    public static class UserExtention
    {
        public static User GetUser(Guid owner)
        {
            UserManager userManager = new UserManager();
            var user = userManager.GetUser(owner);
            return user;
        }
        public static SitefinityProfile GetUserProfile(DynamicContent dcItem)
        {
            UserManager userManager = new UserManager();
            UserProfileManager userProfileManager = new UserProfileManager();
            var user = GetUser(dcItem.Owner);
            if (user!= null)
            {
                var profile = userProfileManager.GetUserProfile<SitefinityProfile>(user);
                return profile;
            }

            return null;
        }

        public static string GetUserAvatarURL(DynamicContent dcItem)
        {
            var profile = UserExtention.GetUserProfile(dcItem);
            if (profile != null)
            {
                ContentLink avatarLink = profile.Avatar;
                if (avatarLink != null && avatarLink.ChildItemId != Guid.Empty)
                {
                    var imageId = avatarLink.ChildItemId;
                    LibrariesManager librariesManager = LibrariesManager.GetManager();
                    Image avatar = librariesManager.GetImage(imageId);
                    if (avatar != null && !string.IsNullOrWhiteSpace(avatar.Url))
                    {
                        return avatar.Url;
                    }
                }
            }

            return string.Empty;
        }
    }
}