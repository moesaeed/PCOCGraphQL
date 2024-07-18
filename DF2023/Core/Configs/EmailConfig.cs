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

        [ConfigurationProperty("OTPEmailSubject",
            DefaultValue = "Your OTP is ")]
        [ObjectInfo(
            Title = "OTP Email Subject"
            )
        ]
        public string OTPEmailSubject
        {
            get => this["OTPEmailSubject"] as string;
            set => this["OTPEmailSubject"] = value;
        }

        [ConfigurationProperty("OTPEmailMessageContentBlock",
            DefaultValue = "OTP")]
        [ObjectInfo(
            Title = "OTP Email Message Content Block"
            )
        ]
        public string OTPEmailMessageContentBlock
        {
            get => this["OTPEmailMessageContentBlock"] as string;
            set => this["OTPEmailMessageContentBlock"] = value;
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