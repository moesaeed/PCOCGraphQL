using System.Configuration;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Localization;

namespace DF2023.Core.Configs
{
    public class OTPConfig : ConfigSection
    {
        public OTPConfig() : base()
        {
        }

        [ConfigurationProperty(nameof(EndpointHeaderValue),
            DefaultValue = "074c0be6a99521b2ac83bf49b9808a3f")]
        [ObjectInfo(
            Title = "OTP end point header value",
            Description = "The Authorization header value expected when post/get OTP")
        ]
        public string EndpointHeaderValue
        {
            get
            {
                return this[nameof(EndpointHeaderValue)] as string;
            }
            set
            {
                this[nameof(EndpointHeaderValue)] = value;
            }
        }

        [ConfigurationProperty(nameof(EndpointHeaderKey),
            DefaultValue = "X-Authorization")]
        [ObjectInfo(
            Title = "OTP end point header key",
            Description = "The Authorization header key expected when post/get OTP")
        ]
        public string EndpointHeaderKey
        {
            get
            {
                return this[nameof(EndpointHeaderKey)] as string;
            }
            set
            {
                this[nameof(EndpointHeaderKey)] = value;
            }
        }
    }
}