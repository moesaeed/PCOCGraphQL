using System;

namespace DF2023.WebPageModel
{
    public class DelegationModel:BaseModel
    {
        public DelegationModel() { }

        public string TitleAr { get; set; }

        public string ContactName { get; set; }
      
        public string ContactPhoneNumber { get; set; }

        public string ContactEmail { get; set; }
       
        public string SecondaryEmail { get; set; }
        
        public string IsSingle { get; set; }

        public int NumberOfOfficialDelegates { get; set; }

        public int RemainingNumberOfOfficialDelegates { get; set; }
        
        public string InvitationDate { get; set; }
       
        public Guid Entity { get; set; }
       
        public Guid ServicesLevel { get; set; }

        public Guid Country { get; set; }

        public string DelegationJSON { get; set; }

        public Guid SystemParentId { get; set; }
    }
}