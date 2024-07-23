using DF2023.Core.Constants;
using DF2023.Core.Extensions;
using DF2023.Mvc.Models;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.RelatedData;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace DF2023.Core.Custom
{
    public class GuestManager : ContentHandler
    {
        public bool IsNewGuest { get; set; }
        public Guid DelegationId { get; set; }
        public DynamicModuleManager DynamicModuleManager { get; set; }
        private DynamicContent _delegation;
        private DateTime _registrationDate { get; set; }

        public override bool IsDataValid(Dictionary<string, object> contextValue, out string errorMsg)
        {
            errorMsg = null;

            bool isGuestDuplicate = CheckGuestDuplicates(contextValue, out errorMsg);
            if (isGuestDuplicate)
            {
                return false;
            }

            var id = contextValue.ContainsKey("id") ? Guid.Parse(contextValue["id"].ToString()) : Guid.Empty;
            if (id == Guid.Empty)
            {
                errorMsg = GetGuestDelegation(contextValue);
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    return false;
                }

                IsNewGuest = true;

                errorMsg = AllowToAddGuest(contextValue);
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    return false;
                }

                var email = contextValue.ContainsKey(Guest.Email.SetFirstLetterLowercase()) ? contextValue[Guest.Email.SetFirstLetterLowercase()].ToString() : string.Empty;
                if (string.IsNullOrWhiteSpace(email))
                {
                    errorMsg = "Email can't be null";
                    return false;
                }
            }

            return true;
        }

        public override void PreProcessData(Dictionary<string, object> contextValue)
        {
            var firstName = contextValue.ContainsKey("firstName") ? contextValue["firstName"].ToString() : string.Empty;
            var lastName = contextValue.ContainsKey("lastName") ? contextValue["lastName"].ToString() : string.Empty;
            string fullName = string.Empty;
            if (!string.IsNullOrWhiteSpace(firstName))
            {
                fullName = firstName;
            }
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                fullName += " " + lastName;
            }

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                TitleValue = fullName;
            }
            else
            {
                TitleValue = Guid.NewGuid().ToString();
            }
        }

        public override void DuringProcessData(DynamicContent item, Dictionary<string, object> contextValue)
        {
            if (IsNewGuest)
            {
                GuestJson guest = new GuestJson();
                guest.DelegationID = DelegationId;

                FlatGuest flatGuest = new FlatGuest();
                flatGuest.Guest = new List<GuestJson>() { guest };

                var json = JsonSerializer.Serialize(flatGuest);
                item.SetValue(Guest.GuestJSON, json);
                DateTime? invitationDate = _delegation.GetValue<DateTime?>(Delegation.InvitationDate);
                if (invitationDate.HasValue && invitationDate.Value > DateTime.MinValue)
                {
                    item.SetValue(Guest.InvitationDate, invitationDate);
                }
            }
        }

        public override void PostProcessData(DynamicContent item, Dictionary<string, Object> contextValue = null)
        {
            if (_registrationDate > DateTime.MinValue)
            {
                item.SetValue(Guest.RegistrationDate, _registrationDate);
            }

            if (IsNewGuest)
            {
                bool result = RelatedGuestToDelegation(item, contextValue);

                if (result)
                {
                    //Set permissions for the delegation
                    var manager = ManagerBase.GetMappedManager(item.GetType().FullName);
                    Telerik.Sitefinity.Security.Model.ISecuredObject secureObject = item;
                    manager.BreakPermiossionsInheritance(secureObject);
                    PermissionExtensions.ClearPermission(item);
                    // TODO: What if PCOC user was creating/adding the guest to exisiting delegation
                    PermissionExtensions.SetPermission(item, new List<Guid>() { item.Owner }, new List<string>() { UserRoles.PCOC });
                    manager.SaveChanges();
                }
            }
        }

        public override void PostProcessRelateItem(DynamicContent item, string normalizedFieldName, IDataItem toRelate)
        {
            if (normalizedFieldName == Guest.GuestStatus
                && toRelate.GetType().ToString() == Gueststatus.GueststatusDynamicTypeName)
            {
                bool isGuestAdmin = UserExtensions.IsCurrentUserInRole(UserRoles.GuestAdmin);
                if (isGuestAdmin)
                {
                    var statusDynamicContent = toRelate as DynamicContent;
                    var status = statusDynamicContent?.GetValue<string>(Gueststatus.Title);

                    if (!string.IsNullOrWhiteSpace(status) && status == Gueststatus.Registered)
                    {
                        _registrationDate = DateTime.UtcNow;
                    }
                }
            }
        }

        private string GetGuestDelegation(Dictionary<string, object> contextValue)
        {
            string guestJson = contextValue.ContainsKey(Guest.GuestJSON.SetFirstLetterLowercase()) ? contextValue[Guest.GuestJSON.SetFirstLetterLowercase()].ToString() : null;
            if (string.IsNullOrWhiteSpace(guestJson))
            {
                return "Delegation can't be null";
            }

            Guid tempDelegationId = Guid.Empty;
            if (Guid.TryParse(guestJson, out tempDelegationId) == false)
            {
                return "Not valid delegation Id";
            }

            DelegationId = tempDelegationId;
            DynamicModuleManager = DynamicModuleManager.GetManager();
            _delegation = DelegationManager.GetDelegation(DelegationId, DynamicModuleManager);

            if (_delegation != null)
            {
                return null;
            }

            return "Not valid delegation";
        }

        private string AllowToAddGuest(Dictionary<string, object> contextValue)
        {
            var numberOfOfficialDelegates = _delegation.GetValue<decimal?>(Delegation.NumberOfOfficialDelegates);
            int counter = (int)numberOfOfficialDelegates.GetValueOrDefault(0);
            var guests = _delegation.GetRelatedItems(Delegation.Guests).Count();
            var isSingleDelegation = _delegation.GetValue<bool?>(Delegation.IsSingle) ?? false;
            if ((isSingleDelegation && guests > 0) || (counter <= guests))
            {
                return "You are not allowed to add more guests";
            }

            return null;
        }

        private bool RelatedGuestToDelegation(DynamicContent item, Dictionary<string, Object> contextValue)
        {
            var errorMsg = AllowToAddGuest(contextValue);

            if (!string.IsNullOrWhiteSpace(errorMsg))
            {
                DeleteGuest(item);
                return false;
            }

            _delegation.CreateRelation(item, Delegation.Guests);

            return true;
        }

        private void DeleteGuest(DynamicContent item)
        {
            DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager();
            dynamicModuleManager.DeleteDataItem(item);
        }

        private static bool CheckGuestDuplicates(Dictionary<string, object> contextValue, out string errorMsg)
        {
            errorMsg = null;

            var id = contextValue.ContainsKey("id") ? Guid.Parse(contextValue["id"].ToString()) : Guid.Empty;
            var type = TypeResolutionService.ResolveType(Guest.GuestDynamicTypeName);
            var email = contextValue.ContainsKey(Guest.Email.SetFirstLetterLowercase()) ? contextValue[Guest.Email.SetFirstLetterLowercase()].ToString().ToLower() : string.Empty;
            var passportNumber = contextValue.ContainsKey(Guest.PassportNumber.SetFirstLetterLowercase()) ? contextValue[Guest.PassportNumber.SetFirstLetterLowercase()].ToString().ToLower() : string.Empty;
            var dynamicManager = DynamicModuleManager.GetManager();

            // New Guest check
            if (id == Guid.Empty)
            {
                var systemParentId = contextValue.ContainsKey("systemParentId") ? Guid.Parse(contextValue["systemParentId"].ToString()) : Guid.Empty;
                if (systemParentId == Guid.Empty)
                {
                    errorMsg = "Can't create a guest without a parent";
                    return true;
                }

                var convention = GetConventionData(systemParentId, dynamicManager);
                var guests = GetConventionGuests(convention, dynamicManager, type);

                if (guests?.Any() == true)
                {
                    if (!string.IsNullOrWhiteSpace(passportNumber))
                    {
                        var duplicatePassport = guests.Where(x => x.GetValue<string>(Guest.PassportNumber).ToLower() == passportNumber);
                        if (duplicatePassport.Any())
                        {
                            errorMsg = "Please contact the event organizers. An application has already been made with the same passport number";
                            return true;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        var duplicateEmail = guests.Where(x => x.GetValue<string>(Guest.Email).ToLower() == email);
                        if (duplicateEmail.Any())
                        {
                            errorMsg = "Please contact the event organizers. An application has already been made with the same email";
                            return true;
                        }
                    }
                }
            }
            else // Existing Guest check
            {
                if (!string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(passportNumber))
                {
                    var item = dynamicManager.GetDataItems(type).FirstOrDefault(i => i.Id == id);
                    if (item != null)
                    {
                        var convention = GetConventionData(item.SystemParentId, dynamicManager);
                        string currentEmail = item.GetValue<string>(Guest.Email).ToLower();
                        string currentPassportNumber = item.GetValue<string>(Guest.PassportNumber)?.ToLower();

                        if (!string.IsNullOrWhiteSpace(email) && email != currentEmail)
                        {
                            var guests = GetConventionGuests(convention, dynamicManager, type);
                            if (guests?.Any() == true && guests.Where(x => x.GetValue<string>(Guest.Email).ToLower() == email).Any())
                            {
                                errorMsg = "There is a guest with the same email";
                                return true;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(passportNumber) && passportNumber != currentPassportNumber)
                        {
                            var guests = GetConventionGuests(convention, dynamicManager, type);
                            if (guests?.Any() == true && guests.Where(x => x.GetValue<string>(Guest.PassportNumber) == passportNumber).Any())
                            {
                                errorMsg = "Please contact the event organizers. An application has already been made with the same passport number";
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static DynamicContent GetConventionData(Guid parentId, DynamicModuleManager dynamicManager)
        {
            DynamicContent convention = dynamicManager.GetDataItem(TypeResolutionService.ResolveType(Convention.ConventionDynamicTypeName), parentId);
            return convention;
        }

        private static IQueryable<DynamicContent> GetConventionGuests(DynamicContent convention, DynamicModuleManager dynamicManager, Type type)
        {
            if (convention != null && dynamicManager.HasChildItems(convention))
            {
                var guests = dynamicManager.GetChildItems(convention, type)
                    .Where(i => i.Status == ContentLifecycleStatus.Live && i.Visible
                    && i.PublishedTranslations.Any(pt =>
                                                   pt == SystemManager.CurrentContext.Culture.Name));

                return guests;
            }
            return null;
        }

        // Define the dynamicManager with transaction as there may be a lot of operations
        public static void UpdateDelegationGuests(DynamicContent delegation, Dictionary<string, object> data, DynamicModuleManager dynamicManager)
        {
            if (delegation != null && data != null && data.Count > 0)
            {
                var guests = delegation.GetRelatedItems(Delegation.Guests);

                if (guests != null && guests.Count() > 0)
                {
                    List<Guid> guestIds = guests.Select(i => i.Id).ToList();
                    var collection = dynamicManager.GetDataItems(TypeResolutionService.ResolveType(Guest.GuestDynamicTypeName));
                    var list = from cl in collection
                               join itemId in guestIds on cl.Id equals itemId
                               select cl;

                    foreach (var item in list)
                    {
                        foreach (var field in data)
                        {
                            item.SetValue(field.Key, field.Value);
                        }
                    }
                }
            }
        }
    }
}