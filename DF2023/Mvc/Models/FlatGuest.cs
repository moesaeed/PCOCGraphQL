using System;
using System.Collections.Generic;

namespace DF2023.Mvc.Models
{
    public class FlatGuest
    {
        public List<GuestJson> Guest { get; set; }
    }

    public class GuestJson
    {
        public Guid DelegationID { get; set; }
        public List<GuestData> GuestData { get; set; }
    }

    public class GuestData
    {
    }
}