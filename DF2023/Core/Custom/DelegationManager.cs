using DF2023.Core.Constants;
using DF2023.Core.Extensions;
using DF2023.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Security;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Services;

namespace DF2023.Core.Custom
{
    public class DelegationManager : ContentHandler
    {
        public bool IsNewDelegation { get; set; }

        public override bool IsDataValid(Dictionary<string, object> contextValue, out string errorMsg)
        {
            errorMsg = null;
            bool isValid = UserExtensions.IsCurrentUserInRole(UserRoles.PCOC);
            if (isValid == false)
            {
                isValid = UserExtensions.IsCurrentUserInRole(UserRoles.GuestAdmin);
                if (isValid == false)
                {
                    errorMsg = "You don't have permission to create item";
                }
            }

            var id = contextValue.ContainsKey("id") ? Guid.Parse(contextValue["id"].ToString()) : Guid.Empty;
            IsNewDelegation = false;
            // id == Guid.Empty => New delegation and we need to create a user
            if (id == Guid.Empty && isValid)
            {
                IsNewDelegation = true;

                var contactName = contextValue.ContainsKey(Delegation.ContactName.SetFirstLetterLowercase()) ? contextValue[Delegation.ContactName.SetFirstLetterLowercase()].ToString() : string.Empty;
                var email = contextValue.ContainsKey(Delegation.ContactEmail.SetFirstLetterLowercase()) ? contextValue[Delegation.ContactEmail.SetFirstLetterLowercase()].ToString() : string.Empty;
                if (string.IsNullOrWhiteSpace(contactName) || string.IsNullOrWhiteSpace(email))
                {
                    errorMsg = "Contact name and email can't be null";
                    isValid = false;
                }
            }

            return isValid;
        }

        public override void PreProcessData(Dictionary<string, object> contextValue)
        {
            SystemManager.RunWithElevatedPrivilege(d =>
            {
                var id = contextValue.ContainsKey("id") ? Guid.Parse(contextValue["id"].ToString()) : Guid.Empty;
                if (id == Guid.Empty)
                {
                    string transaction = Guid.NewGuid().ToString();
                    var contactName = contextValue.ContainsKey(Delegation.ContactName.SetFirstLetterLowercase()) ? contextValue[Delegation.ContactName.SetFirstLetterLowercase()].ToString() : string.Empty;
                    var email = contextValue.ContainsKey(Delegation.ContactEmail.SetFirstLetterLowercase()) ? contextValue[Delegation.ContactEmail.SetFirstLetterLowercase()].ToString() : string.Empty;
                    if (IsValidEmail(email))
                    {
                        string transactio = Guid.NewGuid().ToString();
                        string password = PasswordGenerator.GenerateStrongPassword(8);
                        MembershipCreateStatus membershipCreateStatus =
                            UserExtensions.CreateUser(email, password, contactName, contactName, transaction);
                        if (membershipCreateStatus != MembershipCreateStatus.Success)
                        {
                            throw new InvalidOperationException(membershipCreateStatus.ToString());
                        }

                        TransactionManager.CommitTransaction(transaction);
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