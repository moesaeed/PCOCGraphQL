using System;

namespace DF2023.Mvc.Models
{
    public class OTPDTO
    {
        public Guid UserID { get; set; }
        public string Email { get; set; }
        public int OTPRequests { get; set; }
        public bool IsLocked { get; set; }
        public string OTPCode { get; set;}
    }
}