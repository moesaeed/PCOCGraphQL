using System;
using System.Linq;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.Security.Claims;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Sitefinity.Model;

namespace DF2023.Core.Helpers
{
    public static class ConnectionsHelper
    {
        public static string GetAuthenticateUserEmail()
        {
            var userId = ClaimsManager.GetCurrentUserId();
            var user = UserManager.GetManager().GetUser(userId);
            return user.Email;
        }
        public static (DynamicContent source, DynamicContent target) ValidateConnection(DynamicModuleManager dynamicManager, string email, string source, string target)
        {
            if (isValidGuid(source, target))
            {
                var initiatorItem = GetSpeaker(dynamicManager, new Guid(source));
                if (initiatorItem == null) return (null, null);//new added
                var targetItem = GetSpeaker(dynamicManager, new Guid(target));
                if (targetItem == null) return (null, null);//new added
                if (isValidConnectionRequest(email, initiatorItem.GetValue("Email")?.ToString()))
                {
                    //check if the connection doesn't exist before, if doesn't exist then 
                    if (!isExistConnection(dynamicManager, source, target))
                        return (initiatorItem, targetItem);
                }
            }
            return (null, null);
            //throw new ArgumentException("Invalid Id");
        }
        private static bool isValidGuid(string source, string target)
        {
            Guid id;
            var isvalidSource = Guid.TryParse(source, out id);
            var isvalidtarget = Guid.TryParse(source, out id);
            if (!isvalidSource || !isvalidtarget)
            {
                return false;
                //throw new ArgumentException("Invalid Id");
            }
            return true;
        }

        private static DynamicContent GetSpeaker(DynamicModuleManager dynamicManager, Guid id)
        {
            var speaker = dynamicManager.GetDataItems(TypeResolutionService.ResolveType(Constants.SpeakerType))
                                         .FirstOrDefault(i => i.Id == id);
            /*if (speaker == null)
            {
                throw new ArgumentException("Invalid Id");
            }*/
            return speaker;
        }

        private static bool isValidConnectionRequest(string authenticatedEmail, string initiatorEmail)
        {
            if (!string.IsNullOrWhiteSpace(authenticatedEmail) && !string.IsNullOrWhiteSpace(initiatorEmail))
            {
                if (authenticatedEmail == initiatorEmail) return true;
            }
            return false;
            //throw new ArgumentException("Invalid Id");
        }

        private static bool isExistConnection(DynamicModuleManager dynamicManager, string source, string target)
        {
            var connection = dynamicManager.GetDataItems(TypeResolutionService.ResolveType(Constants.ConnectionType))
                             .FirstOrDefault(i => i.GetValue<string>("InitiatorID").ToString() == source &&
                                                  i.GetValue<string>("TargetID").ToString() == target &&
                                                  i.Visible && i.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Live);
            if (connection != null) return true;
            return false;
            //if(connection == null) { return false; }
            //throw new ArgumentException("Invalid Id");
        }

        public static string GetTargetEmail(DynamicModuleManager dynamicManager, Guid id)
        {
            var connection = dynamicManager.GetDataItems(TypeResolutionService.ResolveType(Constants.ConnectionType))
                             .FirstOrDefault(i => i.Id == id);
            if (connection != null)
            {
                var targetEmail = connection.GetValue("Email")?.ToString();
                return targetEmail;
            }
            return string.Empty;
        }

        public static void SetPermission(DynamicContent item, Guid userId)
        {
            item.ManagePermissions()
                 .ForUser(userId)
                 .Grant().View()
                 .Grant().Modify()
                 .Grant().Create()
                 .Grant().Delete();
        }
        public static void RemovePermission(DynamicContent item)
        {
            item.ManagePermissions()
                 .ForRole("Editors")
                 .Deny().View()
                 .Deny().Modify()
                 .Deny().Create()
                 .Deny().Delete();
        }
    }
}