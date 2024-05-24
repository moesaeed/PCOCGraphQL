using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DF2023.Mvc.Models
{
    public class PushNotificationViewModel
    {
        public List<string> FcmToken { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Icon { get; set; }
        public Dictionary<string, string> Data { get; set; }
    }
}