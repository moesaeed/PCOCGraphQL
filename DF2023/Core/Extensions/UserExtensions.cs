using DF2023.Core.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.Security;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Model.ContentLinks;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Security.Claims;
using Telerik.Sitefinity.Security.Model;

namespace DF2023.Core.Extensions
{
    public static class UserExtensions
    {
        private const string ProviderName = "";

        public static string GetCurentUserName()
        {
            var identity = ClaimsManager.GetCurrentIdentity();
            var userName = identity.Name;

            return userName;
        }

        public static Guid GetCurentUserId()
        {
            var identity = ClaimsManager.GetCurrentIdentity();
            return identity.UserId;
        }

        public static bool IsCurrentUserInRole(string roleName)
        {
            bool isUserInRole = false;

            Guid userId = GetCurentUserId();
            RoleManager roleManager = RoleManager.GetManager();

            if (userId != Guid.Empty)
            {
                bool roleExists = roleManager.RoleExists(roleName);
                if (roleExists)
                {
                    isUserInRole = roleManager.IsUserInRole(userId, roleName);
                }
            }

            return isUserInRole;
        }

        public static Guid IsUserByEmailInRoles(List<string> roleNames, string email)
        {
            UserManager userManager = UserManager.GetManager();
            RoleManager roleManager = RoleManager.GetManager();
            Guid userID = Guid.Empty;

            using (ElevatedModeRegion elevatedUserRegion = new ElevatedModeRegion(userManager))
            {
                using (ElevatedModeRegion elevatedRoleRegion = new ElevatedModeRegion(roleManager))
                {
                    User user = userManager.GetUserByEmail(email);
                    if (user == null)
                    {
                        return userID;
                    }

                    foreach (var roleName in roleNames)
                    {
                        if (roleManager.RoleExists(roleName) && roleManager.IsUserInRole(user.Id, roleName))
                        {
                            return user.Id;
                        }
                    }
                }
            }

            return userID;
        }

        public static string GetCurrentUserAvatarURL()
        {
            var userId = ClaimsManager.GetCurrentIdentity().UserId;
            if (userId != Guid.Empty)
            {
                UserManager userManager = UserManager.GetManager();
                UserProfileManager profileManager = UserProfileManager.GetManager();

                User user = userManager.GetUser(userId);
                if (user != null)
                {
                    SitefinityProfile profile = profileManager.GetUserProfile<SitefinityProfile>(user);

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
                }
            }

            return string.Empty;
        }

        public static SitefinityProfile GetUserProfileById(this Guid Id)
        {
            SitefinityProfile profile = null;
            if (!Id.Equals(Guid.Empty))
            {
                UserProfileManager profileManager = UserProfileManager.GetManager();
                UserManager userManager = UserManager.GetManager(ProviderName);
                User user = userManager.GetUser(Id);
                if (user != null)
                    profile = profileManager.GetUserProfile<SitefinityProfile>(user);
            }
            return profile;
        }

        public static string GetUserById(this Guid Id)
        {
            string UserName = "";
            if (!Id.Equals(Guid.Empty))
            {
                UserProfileManager profileManager = UserProfileManager.GetManager();
                UserManager userManager = UserManager.GetManager(ProviderName);
                User user = userManager.GetUser(Id);
                if (user != null)
                {
                    SitefinityProfile profile = profileManager.GetUserProfile<SitefinityProfile>(user);
                    UserName = string.Concat(profile.FirstName, " ", profile.LastName);
                }
            }
            return UserName;
        }

        public static bool AddFcmToken(string fcmToken)
        {
            UserProfileManager profileManager = UserProfileManager.GetManager();
            UserManager userManager = UserManager.GetManager();
            Guid userId = GetCurentUserId();
            User user = userManager.GetUser(userId);
            if (user != null)
            {
                SitefinityProfile profile = profileManager.GetUserProfile<SitefinityProfile>(user);
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                if (profile.DoesFieldExist("FCMToken"))
                {
                    string fcmTokenJSON = profile.GetValue<string>("FCMToken");
                    string token = string.Empty;
                    List<String> fcmTokenList = new List<string>();
                    if (!string.IsNullOrEmpty(fcmTokenJSON))
                    {
                        fcmTokenList = javaScriptSerializer.Deserialize<List<string>>(fcmTokenJSON);
                        token = fcmTokenList.Where(x => x == fcmToken).FirstOrDefault();
                    }
                    if (string.IsNullOrEmpty(token))
                    {
                        fcmTokenList.Add(fcmToken);
                        fcmTokenJSON = javaScriptSerializer.Serialize(fcmTokenList);
                        profile.SetValue("fcmToken", fcmTokenJSON);
                        profileManager.SaveChanges();
                    }

                    return true;
                }
            }
            return false;
        }

        public static bool UpdateUserNotificationPreference(Dictionary<string, bool> keyValuePairs)
        {
            UserProfileManager profileManager = UserProfileManager.GetManager();
            UserManager userManager = UserManager.GetManager();
            Guid userId = GetCurentUserId();
            User user = userManager.GetUser(userId);
            if (user != null)
            {
                SitefinityProfile profile = profileManager.GetUserProfile<SitefinityProfile>(user);
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                foreach (var item in keyValuePairs)
                {
                    if (profile.DoesFieldExist(item.Key))
                    {
                        bool fcmTokenJSON = profile.GetValue<bool>(item.Key);
                        profile.SetValue(item.Key, item.Value);
                    }
                }
                profileManager.SaveChanges();
                return true;
            }

            return false;
        }

        public static List<string> GetFcmToken(Guid userId)
        {
            UserProfileManager profileManager = UserProfileManager.GetManager();
            UserManager userManager = UserManager.GetManager();
            User user = userManager.GetUser(userId);
            List<string> fcmTokenList = new List<string>();
            if (user != null)
            {
                SitefinityProfile profile = profileManager.GetUserProfile<SitefinityProfile>(user);
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                string fcmTokenJSON = profile.GetValue<string>("FCMToken");
                string token = string.Empty;
                if (!string.IsNullOrEmpty(fcmTokenJSON))
                {
                    fcmTokenList = javaScriptSerializer.Deserialize<List<string>>(fcmTokenJSON);
                }
            }
            return fcmTokenList;
        }

        public static Dictionary<string, bool> GetUserNotificationPreference()
        {
            Dictionary<string, bool> keyValuePairs = new Dictionary<string, bool>();
            Guid userId = GetCurentUserId();
            UserProfileManager profileManager = UserProfileManager.GetManager();
            UserManager userManager = UserManager.GetManager();
            User user = userManager.GetUser(userId);

            if (user != null)
            {
                SitefinityProfile profile = profileManager.GetUserProfile<SitefinityProfile>(user);
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                //foreach (var item in UserNotificationPreference.Notifications)
                //{
                //    if (profile.DoesFieldExist(item))
                //    {
                //        keyValuePairs.Add(item, profile.GetValue<bool>(item));
                //    }
                //}
            }

            return keyValuePairs;
        }

        public static List<Guid> GetUsersInRole(string roleName)
        {
            List<Guid> guids = new List<Guid>();
            RoleManager roleManager = RoleManager.GetManager();

            if (roleManager.RoleExists(roleName))
            {
                var users = roleManager.GetUsersInRole(roleName).Select(x => x.Id).ToList();
                return users;
            }

            return null;
        }

        public static string GetUserAvatarURL(DynamicContent dcItem)
        {
            var profile = GetUserProfile(dcItem);
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

        public static string GetUserCustomfieldValue(string fieldName, Guid userId)
        {
            UserManager userManager = UserManager.GetManager(ProviderName);
            if (userId != Guid.Empty)
            {
                User user = userManager.GetUser(userId);
                UserProfileManager profileManager = UserProfileManager.GetManager();
                var profile = profileManager.GetUserProfile<SitefinityProfile>(user);
                if (profile == null || profile.DoesFieldExist(fieldName) == false)
                {
                    return string.Empty;
                }
                try
                {
                    string v = profile.GetValue<string>(fieldName);
                    return v;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }

            return string.Empty;
        }

        public static bool SetUserCustomfieldValue(string fieldName, string fieldValue, Guid userId)
        {
            UserManager userManager = UserManager.GetManager();
            if (userId != Guid.Empty)
            {
                User user = userManager.GetUser(userId);
                UserProfileManager profileManager = UserProfileManager.GetManager();
                var profile = profileManager.GetUserProfile<SitefinityProfile>(user);
                if (profile == null || profile.DoesFieldExist(fieldName) == false)
                {
                    return false;
                }
                try
                {
                    profile.SetValue(fieldName, fieldValue);
                    profileManager.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }

        public static List<User> GetUsersInListOfRoles(List<string> roleNames)
        {
            List<User> users = new List<User>();
            var userManager = UserManager.GetManager();
            var roleManager = RoleManager.GetManager();

            var roles = roleManager.GetRoles().Where(r => roleNames.Contains(r.Name)).ToList();

            foreach (var role in roles)
            {
                var usersInRole = roleManager.GetUsersInRole(role.Id).ToList();
                users.AddRange(usersInRole);
            }

            users = users.Distinct().ToList();

            return users;
        }

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
            if (user != null)
            {
                var profile = userProfileManager.GetUserProfile<SitefinityProfile>(user);
                return profile;
            }

            return null;
        }

        public static User GetUserByEmail(string email)
        {
            var userManager = UserManager.GetManager();
            var user = userManager.GetUsers().FirstOrDefault(u => u.Email == email);
            return user;
        }

        public static MembershipCreateStatus CreateUser(string email, string password, string firstName, string lastName, string transaction, string guestData = null, string secretQuestion = null, string secretAnswer = null, bool isApproved = true)
        {
            UserManager userManager = UserManager.GetManager("", transaction);
            UserProfileManager profileManager = UserProfileManager.GetManager(UserProfileManager.GetDefaultProviderName(), transaction);
            MembershipCreateStatus status = MembershipCreateStatus.Success;

            User existingUser = userManager.GetUsers().FirstOrDefault(u => u.Email == email);

            bool isUserExist = existingUser != null;
            if (isUserExist == false)
            {
                //return MembershipCreateStatus.DuplicateUserName;
                existingUser = userManager.CreateUser(email, password, email, secretQuestion, secretAnswer, isApproved, null, out status);
                if (status != MembershipCreateStatus.Success)
                {
                    return status;
                }
            }

            if (status == MembershipCreateStatus.Success || isUserExist)
            {
                var profile = profileManager.GetUserProfile<SitefinityProfile>(existingUser);
                if (profile == null)
                {
                    profile = profileManager.CreateProfile(existingUser, Guid.NewGuid(), typeof(SitefinityProfile)) as SitefinityProfile;
                }

                if (profile != null)
                {
                    profile.SetValue("FirstName", firstName);
                    profile.SetValue("LastName", lastName);
                    profile.SetValue("Nickname", $"{firstName} {lastName} {password}");
                }
                if (guestData != null)
                {
                    profile.SetValue(Others.UserCustomField, guestData);
                }

                RoleExtensions.AddUserToRoles(existingUser,
                    new List<string>() {
                 UserRoles.GuestAdmin}, transaction);
            }

            return status;
        }
    }
}