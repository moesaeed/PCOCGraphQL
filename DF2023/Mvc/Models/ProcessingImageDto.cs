using System.Text.Json.Serialization;

namespace DF2023.Mvc.Models
{
    public class ProcessingImageDto
    {
        public string Image { get; set; }
    }

    public class PassportImageResult
    {
        public bool IsValid { get; set; }
        public int OverallStatus { get; set; }
        public int DetailsOpticalStatus { get; set; }
        public string DocumentCategory { get; set; }
        public string DocumentType { get; set; }
        //[JsonIgnore]
        //public string Portrait { get; set; } // Potentially null based on JSON structure
        //[JsonIgnore]
        //public string Signature { get; set; } // Potentially null based on JSON structure
        //public string DocumentFrontSide { get; set; } // Potentially null based on JSON structure
        public string MRZLines { get; set; } // Potentially null based on JSON structure
        public string DocumentClassCode { get; set; } // Potentially null based on JSON structure
        public string IssuingStateCode { get; set; } // Potentially null based on JSON structure
        public string IssuingStateCodeThreeLetters { get; set; } // Potentially null based on JSON structure
        public string Surname { get; set; } // Potentially null based on JSON structure
        public string GivenName { get; set; } // Potentially null based on JSON structure
        public string DocumentNumber { get; set; } // Potentially null based on JSON structure
        public string NationalityCode { get; set; } // Potentially null based on JSON structure
        public string NationalityCodeThreeLetters { get; set; } // Potentially null based on JSON structure
        public string DateOfBirth { get; set; } // Potentially null based on JSON structure
        public string Sex { get; set; } // Potentially null based on JSON structure
        public string DateOfExpiry { get; set; } // Potentially null based on JSON structure
        public string PersonalNumber { get; set; }
        public string Age { get; set; }
        public string IssuingState { get; set; }
        public string MRZType { get; set; }
        public string Nationality { get; set; }
        public string DateOfIssue { get; set; } // Potentially null based on JSON structure
        public string IssuingAuthority { get; set; } // Potentially null based on JSON structure
        public string AgeAtIssue { get; set; } // Potentially null based on JSON structure
        public string YearsSinceIssue { get; set; } // Potentially null based on JSON structure
        public string PlaceOfBirth { get; set; } // Potentially null based on JSON structure
    }
}