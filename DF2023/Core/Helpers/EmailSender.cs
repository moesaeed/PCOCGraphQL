using DF2023.Core.Configs;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Configuration;

namespace DF2023.Core.Helpers
{
    public class EmailSender
    {
        public static bool Send(List<string> recipients, string subject, string emailMessage)
        {
            try
            {
                var config = Config.Get<EmailConfig>();
                var apiKey = config.SendGridApiKey;
                var client = new SendGridClient(apiKey);
                string emailSenderName = config.EmailSenderName;
                string email = config.Email;
                var from = new EmailAddress(email, emailSenderName);

                List<EmailAddress> tos = new List<EmailAddress>();
                foreach (string item in recipients)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        tos.Add(new EmailAddress(item));
                    }
                }

                var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, null, emailMessage, false);
                var res = client.SendEmailAsync(msg).Result;

                if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Log.Write("SendGrid client could not authorize with the provided API key", ConfigurationPolicy.ABTestingTrace);
                }

                return res.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Log.Write("Unexpected error occurred while sending the email \r\n" + ex.Message, ConfigurationPolicy.ABTestingTrace);
                return false;
            }
        }
    }
}