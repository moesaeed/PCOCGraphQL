using DF2023.Core.Constants;
using DF2023.Core.Extensions;
using DF2023.Core.Helpers;
using DF2023.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Security;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace DF2023.Core.Custom
{
    public class DelegationManager : ContentHandler
    {
        public bool IsNewDelegation { get; set; }

        public override bool IsDataValid(Dictionary<string, object> contextValue, out string errorMsg)
        {
            errorMsg = null;

            if (!UserExtensions.IsCurrentUserInRole(UserRoles.PCOC) && !UserExtensions.IsCurrentUserInRole(UserRoles.GuestAdmin))
            {
                errorMsg = "You don't have permission to create item";
                return false;
            }

            bool existingEmail = IsDelegationWithSameEmailExist(contextValue, out errorMsg);
            if (existingEmail)
            {
                return false;
            }

            var id = contextValue.ContainsKey("id") ? Guid.Parse(contextValue["id"].ToString()) : Guid.Empty;
            if (id == Guid.Empty)
            {
                IsNewDelegation = true;

                var contactName = contextValue.ContainsKey(Delegation.ContactName.SetFirstLetterLowercase()) ? contextValue[Delegation.ContactName.SetFirstLetterLowercase()].ToString() : string.Empty;
                var email = contextValue.ContainsKey(Delegation.ContactEmail.SetFirstLetterLowercase()) ? contextValue[Delegation.ContactEmail.SetFirstLetterLowercase()].ToString() : string.Empty;
                if (string.IsNullOrWhiteSpace(contactName) || string.IsNullOrWhiteSpace(email))
                {
                    errorMsg = "Contact name and email can't be null";
                    return false;
                }

                var title = contextValue.ContainsKey("title") ? contextValue["title"].ToString() : string.Empty;
                if (string.IsNullOrWhiteSpace(title))
                {
                    TitleValue = $"{contactName} - {email}";
                }
            }

            return true;
        }

        public override void PreProcessData(Dictionary<string, object> contextValue)
        {
            SystemManager.RunWithElevatedPrivilege(d =>
            {
                var id = contextValue.ContainsKey("id") ? Guid.Parse(contextValue["id"].ToString()) : Guid.Empty;
                if (id == Guid.Empty)
                {
                    string transaction = Guid.NewGuid().ToString();
                    var contactName = contextValue.ContainsKey(Delegation.ContactName.SetFirstLetterLowercase()) ?
                    contextValue[Delegation.ContactName.SetFirstLetterLowercase()].ToString() : string.Empty;

                    var email = contextValue.ContainsKey(Delegation.ContactEmail.SetFirstLetterLowercase()) ?
                    contextValue[Delegation.ContactEmail.SetFirstLetterLowercase()].ToString() : string.Empty;

                    if (IsValidEmail(email))
                    {
                        string password = PasswordGenerator.GenerateStrongPassword(8);
                        MembershipCreateStatus membershipCreateStatus =
                            UserExtensions.CreateUser(email, password, contactName, contactName, transaction);
                        if (membershipCreateStatus != MembershipCreateStatus.Success)
                        {
                            throw new InvalidOperationException(membershipCreateStatus.ToString());
                        }

                        TransactionManager.CommitTransaction(transaction);
                    }
                    else
                    {
                        throw new NoStackTraceException("Not valid email");
                    }
                }
            });
        }

        public override void PostProcessData(DynamicContent item)
        {
            if (IsNewDelegation)
            {
                var email = item.GetValue<string>(Delegation.ContactEmail).ToString();
                Guid delegationID = item.Id;
                Guid conventionID = item.SystemParentId;

                var user = UserExtensions.GetUserByEmail(email);
                if (user != null && user.Id != Guid.Empty)
                {
                    string data = UserExtensions.GetUserCustomfieldValue(Others.UserCustomField, user.Id);
                    Dictionary<Guid, Guid> keyValuePairs = new Dictionary<Guid, Guid>();
                    if (!string.IsNullOrWhiteSpace(data))
                    {
                        keyValuePairs = ConvertStringToDictionary(data);

                        // we assume the user has a delegation in the convention, and the delegation got deleted, and the admin try to create a new one
                        if (keyValuePairs.ContainsKey(conventionID))
                        {
                            keyValuePairs[conventionID] = delegationID;
                        }
                        else
                        {
                            keyValuePairs.Add(conventionID, delegationID);
                        }
                    }
                    else
                    {
                        keyValuePairs.Add(conventionID, delegationID);
                    }

                    string kvp = ConvertDictionaryToString(keyValuePairs);

                    string transaction = Guid.NewGuid().ToString();
                    UserExtensions.SetUserCustomfieldValue(Others.UserCustomField, kvp, user.Id);

                    SystemManager.RunWithElevatedPrivilege(d =>
                    {
                        TransactionManager.CommitTransaction(transaction);
                    });
                }
            }
        }

        public static bool IsDelegationWithSameEmailExist(Dictionary<string, object> contextValue, out string errorMsg)
        {
            errorMsg = null;

            var id = contextValue.ContainsKey("id") ? Guid.Parse(contextValue["id"].ToString()) : Guid.Empty;
            var dynamicManager = DynamicModuleManager.GetManager();
            var type = TypeResolutionService.ResolveType(Delegation.DelegationDynamicTypeName);
            var email = contextValue.ContainsKey(Delegation.ContactEmail.SetFirstLetterLowercase()) ? contextValue[Delegation.ContactEmail.SetFirstLetterLowercase()].ToString().ToLower() : string.Empty;

            if (id == Guid.Empty)
            {
                var systemParentId = contextValue.ContainsKey("systemParentId") ? Guid.Parse(contextValue["systemParentId"].ToString()) : Guid.Empty;
                if (systemParentId == Guid.Empty)
                {
                    errorMsg = "Can't create a delegation without a parent";
                    return true;
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    errorMsg = "Email can't be null";
                    return true;
                }

                DynamicContent convention = dynamicManager.GetDataItem(TypeResolutionService.ResolveType(Convention.ConventionDynamicTypeName), systemParentId);
                if (convention != null && dynamicManager.HasChildItems(convention))
                {
                    var delegations = dynamicManager.GetChildItems(convention, type)
                        .Where(i => i.Status == ContentLifecycleStatus.Live && i.Visible
                                    && i.PublishedTranslations.Any(pt => pt == SystemManager.CurrentContext.Culture.Name))
                        .FirstOrDefault(dc => dc.GetValue<string>(Delegation.ContactEmail).ToLower() == email
                        );

                    if (delegations != null)
                    {
                        errorMsg = "There is a delegation in this convention with the same email";
                        return true;
                    }
                }
            }
            else
            {
                var item = dynamicManager.GetDataItems(type).FirstOrDefault(i => i.Id == id);
                string currentEmail = item.GetValue<string>(Delegation.ContactEmail).ToLower();
                if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(currentEmail) && email != currentEmail)
                {
                    errorMsg = "You can't change email address";
                    return true;
                }
            }

            return false;
        }

        public static bool IsValidEmail(string email)
        {
            string emailRegex = @"^([\w\.\-]+)@([\w\-]+)\.([a-zA-Z]{2,6})$";
            return Regex.IsMatch(email, emailRegex);
        }

        public static string ConvertDictionaryToString(Dictionary<Guid, Guid> dictionary)
        {
            List<string> keyValueStrings = new List<string>();

            foreach (var kvp in dictionary)
            {
                keyValueStrings.Add($"{kvp.Key}:{kvp.Value}");
            }

            return string.Join(", ", keyValueStrings);
        }

        public static Dictionary<Guid, Guid> ConvertStringToDictionary(string str)
        {
            Dictionary<Guid, Guid> dictionary = new Dictionary<Guid, Guid>();
            string[] keyValuePairs = str.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string kvp in keyValuePairs)
            {
                string[] parts = kvp.Split(':');
                if (parts.Length == 2)
                {
                    Guid key = Guid.Parse(parts[0]);
                    Guid value = Guid.Parse(parts[1]);
                    dictionary.Add(key, value);
                }
            }

            return dictionary;
        }
    }
}