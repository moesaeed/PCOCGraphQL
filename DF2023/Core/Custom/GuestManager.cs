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

        private string AllowToAddGuest(Dictionary<string, object> contextValue)
        {
            string errorMsg = null;
            // Check guest json for delegation ID, Get delegation by ID, check number of offical delegation. For single delegation, allow only one guest
            var delegationID = contextValue.ContainsKey(Guest.GuestJSON.SetFirstLetterLowercase()) ? Guid.Parse(contextValue[Guest.GuestJSON.SetFirstLetterLowercase()].ToString()) : Guid.Empty;

            if (delegationID != Guid.Empty)
            {
                var dynamicManager = DynamicModuleManager.GetManager();
                DynamicContent delegation = DelegationManager.GetDelegation(delegationID, dynamicManager);
                if (delegation != null)
                {
                    var numberOfOfficialDelegates = delegation.GetValue<decimal?>(Delegation.NumberOfOfficialDelegates);
                    int counter = (int)numberOfOfficialDelegates.GetValueOrDefault(0);
                    var guests = delegation.GetRelatedItems(Delegation.Guests).Count();
                    var isSingleDelegation = delegation.GetValue<bool?>(Delegation.IsSingle) ?? false;
                    if ((isSingleDelegation && guests > 0) || (counter <= guests))
                    {
                        return errorMsg = "You are not allowed to add more guests";
                    }
                }
                else
                {
                    return errorMsg = "Not valid delegation";
                }
            }
            else
            {
                return errorMsg = "Not valid delegation";
            }

            return errorMsg;
        }

        public override void PostProcessData(DynamicContent item, Dictionary<string, Object> contextValue = null)
        {
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

        private bool RelatedGuestToDelegation(DynamicContent item, Dictionary<string, Object> contextValue)
        {
            Guid delegationId = Guid.Empty;
            if (contextValue.ContainsKey(Guest.GuestJSON))
            {
                string guestJson = contextValue.ContainsKey(Guest.GuestJSON.SetFirstLetterLowercase()) ? contextValue[Guest.GuestJSON.SetFirstLetterLowercase()].ToString() : null;
                if (!string.IsNullOrWhiteSpace(guestJson))
                {
                    var guest = JsonSerializer.Deserialize<FlatGuest>(guestJson);
                    if (guest != null && guest.Guest != null && guest.Guest.Count() > 0)
                    {
                        delegationId = guest.Guest[0].DelegationID;
                    }
                }

                try
                {
                    var errorMsg = AllowToAddGuest(contextValue);
                    if (!string.IsNullOrWhiteSpace(errorMsg))
                    {
                        return false;
                    }

                    if (delegationId != Guid.Empty)
                    {
                        var dynamicManager = DynamicModuleManager.GetManager();
                        DynamicContent delegationItem = DelegationManager.GetDelegation(delegationId, dynamicManager, true);
                        if (delegationItem != null)
                        {
                            delegationItem.CreateRelation(item, Delegation.Guests);

                            return true;
                        }
                        else
                        {
                            DeleteGuest(item);
                        }
                    }
                    else
                    {
                        DeleteGuest(item);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return false;
        }

        private void DeleteGuest(DynamicContent item)
        {
            DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager();
            dynamicModuleManager.DeleteDataItem(item);
        }

        public override void PreProcessData(Dictionary<string, object> contextValue)
        {
            object guestStatus = contextValue.ContainsKey(Guest.GuestStatus.SetFirstLetterLowercase()) ?
                    contextValue[Guest.GuestStatus.SetFirstLetterLowercase()] : null;

            if (guestStatus != null)
            {
                var newDicrionaryList = guestStatus as object[];
                foreach (var newObject in newDicrionaryList)
                {
                    var newObjectAsDictionary = newObject.ToObjectDictionary();
                    var id = newObjectAsDictionary.ContainsKey("id") ? Guid.Parse(newObjectAsDictionary["id"].ToString()) : Guid.Empty;
                    if (id != Guid.Empty)
                    {
                        var type = TypeResolutionService.ResolveType(Gueststatus.GueststatusDynamicTypeName);
                        var dynamicManager = DynamicModuleManager.GetManager();
                        var relatedGuestStatus = dynamicManager.GetDataItems(type).FirstOrDefault(i => i.Id == id);
                        var status = relatedGuestStatus?.GetValue<string>(Gueststatus.Title);

                        if (!string.IsNullOrWhiteSpace(status) && status == Gueststatus.Registered)
                        {
                            contextValue[Guest.RegistrationDate] = DateTime.UtcNow;
                        }
                    }
                }
            }
        }

        public static bool CheckGuestDuplicates(Dictionary<string, object> contextValue, out string errorMsg)
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

        public override void DuringProcessData(DynamicContent item, Dictionary<string, object> contextValue)
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
                item.SetValue(Guest.Title, fullName);
            }

            var title = contextValue.ContainsKey(Guest.Title) ? contextValue[Guest.Title].ToString() : string.Empty;
            if (string.IsNullOrWhiteSpace(fullName))
            {
                item.SetValue(Guest.Title, item.Id.ToString());
            }

            var delegationID = contextValue.ContainsKey(Guest.GuestJSON.SetFirstLetterLowercase()) ? Guid.Parse(contextValue[Guest.GuestJSON.SetFirstLetterLowercase()].ToString()) : Guid.Empty;

            if (delegationID != Guid.Empty)
            {
                var originalGuestJson = item.GetValue(Guest.GuestJSON);

                //GuestJSOn is empty
                if (originalGuestJson == null || string.IsNullOrWhiteSpace(originalGuestJson.ToString()))
                {
                    GuestJson guest = new GuestJson();
                    guest.DelegationID = delegationID;

                    FlatGuest flatGuest = new FlatGuest();
                    flatGuest.Guest = new List<GuestJson>() { guest };

                    contextValue[Guest.GuestJSON.SetFirstLetterLowercase()] = JsonSerializer.Serialize(flatGuest);
                }
                else if (!string.IsNullOrWhiteSpace(originalGuestJson.ToString()))
                {
                    var flatGuest = JsonSerializer.Deserialize<FlatGuest>(originalGuestJson.ToString());
                    if (flatGuest != null && flatGuest.Guest?.Count > 0)
                    {
                        //TODO: Fix this logic - this logic consider only one guest - future: can support multi guests
                        foreach (var guestJson in flatGuest.Guest)
                        {
                            if (guestJson.DelegationID == delegationID)
                            {
                                contextValue[Guest.GuestJSON.SetFirstLetterLowercase()] = JsonSerializer.Serialize(flatGuest);
                                return;
                            }
                            else
                            {
                                flatGuest.Guest.Add(new GuestJson() { DelegationID = delegationID });
                                contextValue[Guest.GuestJSON.SetFirstLetterLowercase()] = JsonSerializer.Serialize(flatGuest);
                                return;
                            }
                        }
                    }
                }
            }
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