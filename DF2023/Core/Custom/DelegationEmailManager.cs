using DF2023.Core.Constants;
using DF2023.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.RelatedData;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace DF2023.Core.Custom
{
    public class DelegationEmailManager
    {
        public static bool SendInvitationEmail(Guid delegationID, Guid conventionID, out string errorMsg, bool isConformationEmail = false)
        {
            errorMsg = null;
            try
            {
                string transaction = Guid.NewGuid().ToString();
                var dynamicManager = DynamicModuleManager.GetManager("", transaction);
                DynamicContent convention = dynamicManager.GetDataItem(TypeResolutionService.ResolveType(Convention.ConventionDynamicTypeName), conventionID);
                DynamicContent delegation = dynamicManager.GetDataItem(TypeResolutionService.ResolveType(Delegation.DelegationDynamicTypeName), delegationID);

                if (convention != null && delegation != null)
                {
                    bool result = false;
                    bool customInvitation = convention.GetValue<bool>(Convention.UseCustomInvitation);
                    string subject = null;
                    string emailMessage = null;

                    // Convention without custom invitation has no conformation email
                    if (customInvitation == false && isConformationEmail)
                    {
                        return true;
                    }

                    if (customInvitation)
                    {
                        var service = delegation.GetRelatedItems(Delegation.ServicesLevel)?.FirstOrDefault();

                        if (service != null)
                        {
                            var customTextConfig = service.GetRelatedParentItems(CustomTextConfig.CustomTextConfigDynamicTypeName, fieldName: CustomTextConfig.ServiceLevel)
                                .OfType<DynamicContent>()
                                .Where(x => x.SystemParentId == conventionID)
                                ?.FirstOrDefault();
                            if (customTextConfig != null)
                            {
                                subject = isConformationEmail ?
                                    customTextConfig.GetValue<string>(CustomTextConfig.ConfirmationEmailSubject) :
                                    customTextConfig.GetValue<string>(CustomTextConfig.InvitationEmailSubject);
                                emailMessage = isConformationEmail ?
                                    customTextConfig.GetValue<string>(CustomTextConfig.ConfirmationEmail) :
                                    customTextConfig.GetValue<string>(CustomTextConfig.InvitationEmail);
                            }
                        }
                    }
                    else
                    {
                        subject = convention.GetValue<string>(Convention.InvitationEmailSubject);
                        emailMessage = convention.GetValue<string>(Convention.InvitationEmail);
                    }
                    List<string> recipients = new List<string>()
                    {
                        delegation.GetValue<string>(Delegation.ContactEmail),
                        delegation.GetValue<string>(Delegation.SecondaryEmail)
                    };

                    var filteredRecipients = recipients
                        .Where(email => !string.IsNullOrWhiteSpace(email))
                        .Distinct()
                        .ToList();

                    if (!string.IsNullOrEmpty(subject)
                        && !string.IsNullOrEmpty(emailMessage)
                        && filteredRecipients.Count > 0)
                    {
                        emailMessage = emailMessage.Replace("[Email]", delegation.GetValue<string>(Delegation.ContactEmail));
                        emailMessage = emailMessage.Replace("[Name]", delegation.GetValue<string>(Delegation.ContactName));
                        result = EmailSender.Send(filteredRecipients, subject, emailMessage);
                    }
                    else
                    {
                        errorMsg = "Email subject, message, and recipients can't be null.";
                        return false;
                    }

                    if (result)
                    {
                        delegation.SetValue(Delegation.InvitationDate, DateTime.UtcNow);

                        Dictionary<string, object> keyValuePairs = new Dictionary<string, object>()
                            {
                                { Guest.InvitationDate, DateTime.UtcNow}
                            };

                        GuestManager.UpdateDelegationGuests(delegation, keyValuePairs, dynamicManager);

                        TransactionManager.CommitTransaction(transaction);

                        return true;
                    }
                    else
                    {
                        errorMsg = "Couldn't send email.";
                    }
                }

                errorMsg = "Couldn't find delegation nor convention.";
                return false;
            }
            catch (Exception ex)
            {
                Log.Write(ex, ConfigurationPolicy.ABTestingTrace);
                return false;
            }
        }
    }
}