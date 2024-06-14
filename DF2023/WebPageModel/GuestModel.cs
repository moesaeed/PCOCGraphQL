using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DF2023.WebPageModel
{
    public class GuestModel:BaseModel
    {
        public string FullNameAr { get; set; }

        public string FirstName { get; set; }

        public string FirstNameAr { get; set; }
        
        public string SecondName { get; set; }
        
        public string SecondNameAr { get; set; }

        public string ThirdName { get; set; }
        
        public string ThirdNameAr { get; set; }

        public string FourthName { get; set; }

        public string FourthNameAr { get; set; }
        
        public string LastName { get; set; }

        public string LastNameAr { get; set; }

        public string Gender { get; set; }
       
        public string DOB { get; set; }

        public string JobTitle { get; set; }

        public string Email { get; set; }

        public string SecondaryEmail { get; set; }

        public string IsLocal { get; set; }
       
        public string PhoneAreaCode { get; set; }

        public string Phone { get; set; }

        public Guid Nationality { get; set; }

        public Guid ResidenceCountry { get; set; }

        public string SourceOfInvitation { get; set; }
        
        public string InvitationDate { get; set; }
       
        public string RegistrationDate { get; set; }

        public Guid DepartureAirport { get; set; }
        public string OrganizationName { get; set; }
        public string SpecialLogisticsRequirements { get; set; }

        public string PersonalPhoto { get; set; }

        public string Notes { get; set; }

        public Guid ServicesLevel { get; set; }

        public Guid SubAttendeeType { get; set; }

        public Guid GuestStatus { get; set; }

        public Guid PassportCountry { get; set; }

        public string PassportPhoto { get; set; }

        public string PassportExpiryDate { get; set; }

        public Guid PassportType { get; set; }

        public string PassportNumber { get; set; }

        public string SubscribeToNewsletter { get; set; }

        public string OpeningCeremony { get; set; }

        public Guid BadgeType { get; set; }

        public string RejectNote { get; set; }

        public string SpeakerPhotograph { get; set; }

        public string EquipmentOrBiography { get; set; }

        public string GalaDinner { get; set; }

        public string BatchLogisticsStatus { get; set; }

        public Guid PersonTitle { get; set; }

        public Guid DelegationMemberType { get; set; }

        public Guid GuestStageStatus { get; set; }

        public Guid SystemParentId { get; set; }

        public GuestModel() { }
    }
}