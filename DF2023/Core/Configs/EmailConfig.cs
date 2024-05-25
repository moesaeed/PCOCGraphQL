using System.Configuration;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Localization;

namespace DF2023.Core.Configs
{
    public class EmailConfig : ConfigSection
    {
        public EmailConfig() : base()
        {
        }

        [ConfigurationProperty("EmailSenderName",
            DefaultValue = "Guest Registration - The State of Qatar")]
        [ObjectInfo(
            Title = "Email sender name",
            Description = "The name of the sender/app")
        ]
        public string EmailSenderName
        {
            get => this["EmailSenderName"] as string;
            set => this["EmailSenderName"] = value;
        }

        [ConfigurationProperty("SendGridApiKey",
            DefaultValue = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAE4wxNitu20v2cpESuwci4XKC0PMuMtc/amOzdqK7mYxsAen4sTprSG7U9gecVab8ydLq3C187zXrFsxAmp/Nb6g==")]
        [ObjectInfo(
            Title = "SendGrid Api Key"
            )
        ]
        public string SendGridApiKey
        {
            get => this["SendGridApiKey"] as string;
            set => this["SendGridApiKey"] = value;
        }

        [ConfigurationProperty("Email",
           DefaultValue = "noreply@gm.gov.qa")]
        [ObjectInfo(
           Title = "Email",
           Description = "The sender email address guests and users will recieve email from")
       ]
        public string Email
        {
            get => this["Email"] as string;
            set => this["Email"] = value;
        }
    }
}