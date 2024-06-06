namespace DF2023.WebPageModel
{
    public class DelegationModel:BaseModel
    {
        public DelegationModel() { }

        public string ContactPhoneNumber { get; set; }

        public string ContactEmail { get; set; }

        public string NumberOfOfficialDelegates { get; set; }

        public string Entity { get; set; }

        public string TitleAr { get; set; }

        public string RemainingNumberOfOfficialDelegates { get; set; }

        public string IsSingle { get; set; }

        public string ServicesLevel { get; set; }

        public string InvitationDate { get; set; }

        public string Country { get; set; }

        public string DelegationJSON { get; set; }

        public string ContactName { get; set; }

        public string SecondaryEmail { get; set; }

        public string SystemParentId { get; set; }
    }
}