﻿using System.Configuration;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Localization;

namespace DF2023.Core.Configs
{
    public class OTPConfig : ConfigSection
    {
        public OTPConfig() : base()
        {
        }

        [ConfigurationProperty("OTPKey",
   DefaultValue = "0TAK/5oLJQlkXNRHAUvsusUV+FYD1TuYKfcOlEkKJ/P1sXtyTEkWlqN6SRMIUIrWhF/dVgu7mw/wOpzFs6zPCg==")]
        [ObjectInfo(
   Title = "OTPKey",
   Description = "The key used to generate OTP")
]
        public string OTPKey
        {
            get => this["OTPKey"] as string;
            set => this["OTPKey"] = value;
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

        [ConfigurationProperty(nameof(MaxAttempts))]
        [ObjectInfo(
            Title = "Max attemp of OTP",
            Description = "The maximum number of attempts then user will get locked")
        ]
        public int MaxAttempts
        {
            get
            {
                int? nullableInt = this[nameof(MaxAttempts)] as int?;
                int maxAtt = nullableInt ?? 0;
                return maxAtt;
            }
            set
            {
                this[nameof(MaxAttempts)] = value;
            }
        }
    }
}