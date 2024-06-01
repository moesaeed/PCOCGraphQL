﻿using DF2023.Core.Constants;
using DF2023.Core.Helpers;
using DocumentFormat.OpenXml.ExtendedProperties;
using OpenAccessRuntime.Relational.metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.RelatedData;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace DF2023.Core.Custom
{
    public class DelegationEmailManager
    {
        public static bool SendInvitationEmail(Guid delegationID, Guid conventionID, out string errorMsg)
        {
            errorMsg = null;
            try
            {
                var dynamicManager = DynamicModuleManager.GetManager();
                DynamicContent convention = dynamicManager.GetDataItem(TypeResolutionService.ResolveType(Convention.ConventionDynamicTypeName), conventionID);
                DynamicContent delegation = dynamicManager.GetDataItem(TypeResolutionService.ResolveType(Delegation.DelegationDynamicTypeName), delegationID);

                if (convention != null && delegation != null)
                {
                    bool result = false;
                    bool customInvitation = convention.GetValue<bool>(Convention.UseCustomInvitation);
                    string subject = null;
                    string emailMessage = null;

                    if (customInvitation)
                    {
                        // TODO: Rawad could you please
                        // 1. Retrieve delegation's service level.
                        // 2. Based on the service level, fetch custom text configurations form the convention.
                        // 3. Extract the invitation email subject and Invitation email content from the retrieved configurations.
                        
                        var service = delegation.GetRelatedItems(Delegation.ServicesLevel)?.FirstOrDefault();
                        if(service != null)
                        {
                            var contentLink = DynamicContentExtension.GetRelationsByChild(service.Id, ServicesLevel.ServicesLevelDynamicTypeName, CustomTextConfig.CustomTextConfigDynamicTypeName)?.FirstOrDefault();
                            if (contentLink != null)
                            {
                                var customTextConfig = dynamicManager.GetDataItem(TypeResolutionService.ResolveType(CustomTextConfig.CustomTextConfigDynamicTypeName), contentLink.ParentItemId);
                                subject = customTextConfig.GetValue<string>(CustomTextConfig.InvitationEmailSubject);
                                emailMessage = customTextConfig.GetValue<string>(CustomTextConfig.InvitationEmail);
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
                        result = EmailSender.Send(filteredRecipients, subject, emailMessage);
                    }
                    else
                    {
                        errorMsg = "Email subject, message, and recipients can't be null.";
                        return false;
                    }

                    if (result)
                    {
                        var InvitationDate = delegation.GetValue<DateTime?>(Delegation.InvitationDate);
                        if (InvitationDate == null || (InvitationDate.HasValue && InvitationDate.Value == DateTime.MinValue))
                        {
                            delegation.SetValue(Delegation.InvitationDate, DateTime.UtcNow);
                            dynamicManager.SaveChanges();
                        }

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