using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using DF2023.WebPageModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DF2023.WebPageHelper
{
    public static class PanelHelper
    {
        #region Delegation

        public static (List<DelegationModel> Results, List<string> Errors) CreateDelegation(string baseUrl, int numberOfDelegationToCreate, Guid parentId, string token, int isSingleStats, string invitationDate)
        {
            var countries = GetDataHelper.GetData(baseUrl, "country");
            var services = GetDataHelper.GetData(baseUrl, "servicesLevel");
            var entites = GetDataHelper.GetData(baseUrl, "entity");
            List<DelegationModel> list = new List<DelegationModel>();
            List<string> listError = new List<string>();
            for (int i = 0; i < numberOfDelegationToCreate; i++)
            {
                var rdCountry = new Random().Next(0, countries.Count - 1);
                var rdService = new Random().Next(0, services.Count - 1);
                var rdEntity = new Random().Next(0, entites.Count - 1);
                var delagationModel = GenerateDelegationModel(parentId, Guid.Parse(entites[rdEntity].ToString()), Guid.Parse(services[rdService].ToString()), Guid.Parse(countries[rdCountry].ToString()), isSingleStats, invitationDate);
                var returnedDelegation = CreateNewDelegation(baseUrl, token, delagationModel);
                if (returnedDelegation != null)
                {
                    if (returnedDelegation.ContainsKey("data"))
                    {
                        list.Add(JsonConvert.DeserializeObject<DelegationModel>(returnedDelegation["data"]["delegationSave"].ToString()));
                    }
                    else
                    {
                        var error = JsonConvert.DeserializeObject<string>(returnedDelegation["error"].ToString());
                        error = $"Failed to save the delegation Title:{delagationModel.Title} , Email:{delagationModel.ContactEmail} , ContactName: {delagationModel.ContactName} \n Exception: {error}";
                        listError.Add(error);
                    }
                }
            }

            return (list, listError);
        }

        private static DelegationModel GenerateDelegationModel(Guid parentId, Guid entity, Guid serviceLevel, Guid country, int isSingleStats, string invitationDate)
        {
            string Title = GenerateName(new Random().Next(1, 20));
            string TitleAr = $"{Title} - Ar";
            string ContactName = GenerateName(new Random().Next(1, 20));
            string ContactPhoneNumber = GenerateNumberAsString(new Random().Next(7, 10));
            string ContactEmail = $"{Title.Replace(" ", "")}@email.com";
            string SecondaryEmail = $"{ContactName.Replace(" ", "")}@email.com";
            string IsSingle = "";
           switch (isSingleStats)
            {
                case 1:
                    IsSingle = "true";
                    break;
                case 0:
                    IsSingle="false";
                    break;
                default:
                    IsSingle = new Random().Next(0, 1) == 0 ? "true" : "false";
                    break;
            }
            int NumberOfOfficialDelegates = new Random().Next(1, 10);
            int RemainingNumberOfOfficialDelegates = new Random().Next(1, 5);
            string InvitationDate = "";
            if (!string.IsNullOrWhiteSpace(invitationDate))
             InvitationDate = GenerateRandomDate(InvitationDate, Convert.ToDateTime(InvitationDate).AddMonths(2).ToString("yyyy-MM-dd"));
            else
                InvitationDate = GenerateRandomDate("2024-08-01", "2024-08-01");
            DelegationModel model = new DelegationModel()
            {
                Title = Title,
                TitleAr = TitleAr,
                ContactName = ContactName,
                ContactPhoneNumber = ContactPhoneNumber,
                ContactEmail = ContactEmail,
                SecondaryEmail = SecondaryEmail,
                IsSingle = IsSingle,
                NumberOfOfficialDelegates = NumberOfOfficialDelegates,
                RemainingNumberOfOfficialDelegates = RemainingNumberOfOfficialDelegates,
                Country = country,
                Entity = entity,
                ServicesLevel = serviceLevel,
                InvitationDate= InvitationDate,
                SystemParentId = parentId
            };
            return model;
        }

        private static JObject CreateNewDelegation(string baseUrl, string token, DelegationModel model)
        {

            string query = $@"mutation {{
                              delegationSave(delegationArg: {{
                                title: ""{model.Title}"",
                                contactEmail: ""{model.ContactEmail}"",
                                contactPhoneNumber: ""{model.ContactPhoneNumber}"",
                                numberOfOfficialDelegates: {model.NumberOfOfficialDelegates},
                                remainingNumberOfOfficialDelegates: {model.RemainingNumberOfOfficialDelegates},
                                isSingle: {model.IsSingle},
                                contactName: ""{model.ContactName}"",
                                secondaryEmail: ""{model.SecondaryEmail}"",
                                entity: {{
                                  id: ""{model.Entity}""
                                }},
                                country: {{
                                  id: ""{model.Country}""
                                }},
                                servicesLevel: {{
                                  id: ""{model.ServicesLevel}""
                                }},
                                systemParentId: ""{model.SystemParentId}""
                              }}) {{
                                id,
                                title,
                                contactEmail,
                                contactPhoneNumber,
                                isSingle,
                                contactName,
                                secondaryEmail
                              }}
                            }}";
            var delegation = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null, token);
            return delegation;
        }

        #endregion

        #region Guests
        public static (List<GuestModel> Results, List<string> Errors) CreateGuest(string baseUrl, int numberOfGuestsToCreate, Guid parentId, string token)
        {
            var countries = GetDataHelper.GetData(baseUrl,"country");
            var services = GetDataHelper.GetData(baseUrl, "servicesLevel");
            var passportType = GetDataHelper.GetData(baseUrl, "passporttype");
            var personTitle = GetDataHelper.GetData(baseUrl, "titlelist");
            var guestStatus = GetDataHelper.GetData(baseUrl, "gueststatus");
            var attendeeType = GetDataHelper.GetData(baseUrl, "subattendeetype");
            var guestStageStatus = GetDataHelper.GetData(baseUrl, "gueststagestatus");
            var badgeType = GetDataHelper.GetData(baseUrl, "badgetype");
            var delegationMemberType = GetDataHelper.GetData(baseUrl, "delegationmembertype");
            var airport = GetDataHelper.GetData(baseUrl, "airport");

            List<GuestModel> list = new List<GuestModel>();
            List<string> listError = new List<string>();
            for (int i = 0; i < numberOfGuestsToCreate; i++)
            {
                var rdCountry = new Random().Next(0, countries.Count - 1);
                var rdService = new Random().Next(0, services.Count - 1);
                var rdPassportType = new Random().Next(0, passportType.Count - 1);
                var rdGuestStatus = new Random().Next(0, guestStatus.Count - 1);
                var rdAttendeeType = new Random().Next(0, attendeeType.Count - 1);
                var rdGuestStageStatus = new Random().Next(0, guestStageStatus.Count - 1);
                var rdBadgeType = new Random().Next(0, badgeType.Count - 1);
                var rdDelegationMemberType = new Random().Next(0, delegationMemberType.Count - 1);
                var rdPersonTitle = new Random().Next(0, personTitle.Count - 1);
                var rdAirport = new Random().Next(0, airport.Count - 1);

                var guestModel = GenerateGuestModel(parentId, Guid.Parse(countries[rdCountry].ToString()), Guid.Parse(services[rdService].ToString()),
                                                    Guid.Parse(passportType[rdPassportType].ToString()), Guid.Parse(guestStatus[rdGuestStatus].ToString()),
                                                    Guid.Parse(attendeeType[rdAttendeeType].ToString()), Guid.Parse(guestStageStatus[rdGuestStageStatus].ToString()),
                                                    Guid.Parse(badgeType[rdBadgeType].ToString()), Guid.Parse(delegationMemberType[rdDelegationMemberType].ToString()),
                                                    Guid.Parse(personTitle[rdPersonTitle].ToString()), Guid.Parse(airport[rdAirport].ToString()));
                var returnedGuest = CreateNewGuest(baseUrl, token, guestModel);
                if (returnedGuest != null)
                {
                    if (returnedGuest.ContainsKey("data"))
                    {
                        list.Add(JsonConvert.DeserializeObject<GuestModel>(returnedGuest["data"]["guestSave"].ToString()));
                    }
                    else
                    {
                        var error = JsonConvert.DeserializeObject<string>(returnedGuest["error"].ToString());
                        error = $"Failed to save the guest Title:{guestModel.Title} , Email:{guestModel.FirstName} \n Exception: {error}";
                        listError.Add(error);
                    }
                }
            }
            return (list, listError);
        }

        private static GuestModel GenerateGuestModel(Guid parentId,Guid country, Guid serviceLevel, Guid passportType,Guid guestStatus,Guid subAttendeeType,Guid guestStageStatus, Guid badgeType, Guid delegationMemberType,Guid personTitle, Guid departureAirport)
        {
            string Title = GenerateName(new Random().Next(1, 20));
            string FullNameAr = GenerateArabicName(new Random().Next(1, 10));
            string FirstName = GenerateName(new Random().Next(1, 15));
            string FirstNameAr = GenerateArabicName(new Random().Next(1, 10));
            string SecondName = GenerateName(new Random().Next(1, 15));
            string SecondNameAr = GenerateArabicName(new Random().Next(1, 10));
            string ThirdName = GenerateName(new Random().Next(1, 15));
            string ThirdNameAr = GenerateArabicName(new Random().Next(1, 10));
            string FourthName = GenerateName(new Random().Next(1, 15));
            string FourthNameAr = GenerateArabicName(new Random().Next(1, 10));
            string LastName = GenerateName(new Random().Next(1, 15));
            string LastNameAr = GenerateArabicName(new Random().Next(1, 10));
            string Gender = new Random().Next(0, 1) == 0 ? "Male" : "Female";
            string DOB = GenerateRandomDate("1950-01-01", "2000-01-01");
            string PhoneAreaCode = GenerateName(new Random().Next(1, 3));
            string PhoneNumber = GenerateNumberAsString(new Random().Next(7, 10));
            string Email = $"{Title}@email.com";
            string SecondaryEmail = $"{Title}_2@email.com";
            string EquipmentOrBiography = GenerateName(new Random().Next(1, 15));
            string GalaDiner = new Random().Next(0, 1) == 0 ? "true" : "false";
            string PassportNumber = GenerateRandomPassportNumber(9);
            string PassportExpiryDate = GenerateRandomDate("2024-06-01", "2026-07-01");
            string InvitationDate = GenerateRandomDate("2024-06-01", "2024-07-01");
            string RegistrationDate = GenerateRandomDate(DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.AddDays(30).ToString("yyyy-MM-dd"));
            string OrganizationName = GenerateName(new Random().Next(1, 20));
            string OpeningCeremony = GenerateName(new Random().Next(1, 20));
            string SourceOfInvitation = GenerateName(new Random().Next(1, 20));
            string BatchLogisticsStatus = GenerateName(new Random().Next(1, 20));
            string IsLocal = new Random().Next(0, 1) == 0 ? "true" : "false";
            string IssubscribeToNewsletter = new Random().Next(0, 1) == 0 ? "true" : "false";

            string JobTitle = GenerateName(new Random().Next(1, 20));
            GuestModel model = new GuestModel()
            {
                Title = Title,
                FullNameAr = FullNameAr,
                FirstName = FirstName,
                FirstNameAr = FirstNameAr,
                SecondName = SecondName,
                SecondNameAr = SecondNameAr,
                ThirdName = ThirdName,
                ThirdNameAr = ThirdNameAr,
                FourthName = FourthName,
                FourthNameAr = FourthNameAr,
                LastName = LastName,
                LastNameAr = LastNameAr,
                Gender = Gender,
                DOB = DOB,
                PhoneAreaCode = PhoneAreaCode,
                Phone = PhoneNumber,
                Email = Email,
                SecondaryEmail = SecondaryEmail,
                EquipmentOrBiography = EquipmentOrBiography,
                GalaDinner = GalaDiner,
                InvitationDate = InvitationDate,
                PassportNumber = PassportNumber,
                PassportExpiryDate = PassportExpiryDate,
                JobTitle = JobTitle,
                PersonTitle = personTitle,
                OrganizationName = OrganizationName,
                OpeningCeremony = OpeningCeremony,
                SourceOfInvitation = SourceOfInvitation,
                BadgeType = badgeType,
                BatchLogisticsStatus = BatchLogisticsStatus,
                DelegationMemberType = delegationMemberType,
                DepartureAirport = departureAirport,
                GuestStageStatus = guestStageStatus,
                IsLocal = IsLocal,
                Nationality = country,
                PassportCountry = country,
                PassportType = passportType,
                RegistrationDate = RegistrationDate,
                ServicesLevel = serviceLevel,
                ResidenceCountry = country,
                GuestStatus = guestStatus,
                SubAttendeeType = subAttendeeType,
                SubscribeToNewsletter = IssubscribeToNewsletter,
                SystemParentId = parentId
            };
            return model;
        }

        private static JObject CreateNewGuest(string baseUrl, string token, GuestModel model)
        {
            string query = $@"mutation
                            {{
                              guestSave(guestArg:
                                {{
                                  title:""{model.Title}"",
                                  fullNameAr:""{model.FullNameAr}"",
                                  firstName:""{model.FirstName}"",
                                  firstNameAr:""{model.FirstNameAr}"",
                                  secondName:""{model.SecondName}"",
                                  secondNameAr:""{model.SecondNameAr}"",
                                  thirdName:""{model.ThirdName}"",
                                  thirdNameAr:""{model.ThirdNameAr}"",
                                  fourthName:""{model.FourthName}"",
                                  fourthNameAr:""{model.FourthNameAr}"",
                                  gender:""{model.Gender}"",
                                  dOB:""{model.DOB}"",
                                  phoneAreaCode:""{model.PhoneAreaCode}"",
                                  phone:""{model.Phone}"",
                                  email:""{model.Email}"",
                                  equipmentOrBiography:""{model.EquipmentOrBiography}"",
                                  galaDinner:{model.GalaDinner},
                                  registrationDate:""{model.RegistrationDate}"",
                                  secondaryEmail:""{model.SecondaryEmail}"",
                                  sourceOfInvitation:""{model.SourceOfInvitation}"",
                                  subscribeToNewsletter:{model.SubscribeToNewsletter},
                                  organizationName:""{model.OrganizationName}"",
                                  openingCeremony:""{model.OpeningCeremony}"",
                                  jobTitle:""{model.JobTitle}"",
                                  batchLogisticsStatus:""{model.BatchLogisticsStatus}"",
                                  specialLogisticsRequirements:""{model.SpecialLogisticsRequirements}"",
                                  isLocal:{model.IsLocal},
                                  nationality:{{
                                    id:""{model.Nationality}""
                                  }},
                                  passportCountry:{{
                                    id:""{model.PassportCountry}""
                                  }},
                                  passportType:{{
                                    id:""{model.PassportType}""
                                  }},
                                  personTitle:
                                  {{
                                    id:""{model.PersonTitle}""
                                  }},
                                  residenceCountry:
                                  {{
                                    id:""{model.ResidenceCountry}""
                                  }},
                                  departureAirport:
                                  {{
                                    id:""{model.DepartureAirport}""
                                  }},
                                  servicesLevel:
                                  {{
                                    id:""{model.ServicesLevel}""
                                  }},
                                  guestStatus:
                                  {{
                                    id:""{model.GuestStatus}""
                                  }},
                                  badgeType:
                                  {{
                                    id:""{model.BadgeType}""
                                  }},
                                  delegationMemberType:
                                  {{
                                    id:""{model.DelegationMemberType}""
                                  }},
                                  subAttendeeType:
                                  {{
                                    id:""{model.SubAttendeeType}""
                                  }},
                                  guestStageStatus:
                                  {{
                                    id:""{model.GuestStageStatus}""
                                  }},
                                  systemParentId:""{model.SystemParentId}""
                                }})
  
                              {{
                                id,
                                title,
                                firstName,
                                lastName,
                                email,
                                isLocal
                              }}
                            }}";
            var guest = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null, token);
            return guest;
        }

        #endregion

        private static string GenerateArabicName(int len)
        {
            Thread.Sleep(5);
            Random r = new Random();

            // Define Arabic consonants and vowels
            string[] consonants = { "ب", "ت", "ث", "ج", "ح", "خ", "د", "ذ", "ر", "ز", "س", "ش", "ص", "ض", "ط", "ظ", "ع", "غ", "ف", "ق", "ك", "ل", "م", "ن", "ه", "و", "ي" };
            string[] vowels = { "ا", "و", "ي" }; // Arabic does not have traditional vowels like in English, so we use these to represent common vowel sounds.

            string Name = "";
            Name += consonants[r.Next(consonants.Length)];
            Name += vowels[r.Next(vowels.Length)];
            int b = 2; // Starts at 2 because the first two characters are already in the name.

            while (b < len)
            {
                Name += consonants[r.Next(consonants.Length)];
                b++;
                if (b < len)
                {
                    Name += vowels[r.Next(vowels.Length)];
                    b++;
                }
            }

            return Name;
        }

        private static string GenerateName(int len)
        {
            Thread.Sleep(5);
            Random r = new Random();
            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x", " " };
            string[] vowels = { "a", "e", "i", "o", "u", "ae", "y", " " };
            string Name = "";
            Name += consonants[r.Next(consonants.Length)].ToUpper();
            Name += vowels[r.Next(vowels.Length)];
            int b = 2; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.
            while (b < len)
            {
                Name += consonants[r.Next(consonants.Length)];
                b++;
                Name += vowels[r.Next(vowels.Length)];
                b++;
            }
            return Name;
        }

        private static string GenerateNumberAsString(int len)
        {
            Thread.Sleep(5);
            Random r = new Random();
            string[] numbers = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string Name = "";
            Name += numbers[r.Next(numbers.Length)];
            int b = 2; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.
            while (b < len)
            {
                Name += numbers[r.Next(numbers.Length)];
                b++;
            }
            return Name;
        }

        private static string GenerateRandomPassportNumber(int len)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder passportNumber = new StringBuilder(len);
            for (int i = 0; i < len; i++)
            {
                passportNumber.Append(chars[random.Next(chars.Length)]);
            }
            return passportNumber.ToString();
        }
       
        private static string GenerateRandomDate(string startDate, string endDate)
        {
            DateTime start = DateTime.ParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime end = DateTime.ParseExact(endDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            if (start > end)
            {
                throw new ArgumentException("Start date must be earlier than end date");
            }
            Random random = new Random();
            int range = (end - start).Days;
            return start.AddDays(random.Next(range + 1)).ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
       
        private static string GenerateRandomAirport()
        {
            Random random = new Random();
            List<string> airports = new List<string>
                {
                    // North America
                    "ATL", // Hartsfield-Jackson Atlanta International Airport, USA
                    "LAX", // Los Angeles International Airport, USA
                    "ORD", // Chicago O'Hare International Airport, USA
                    "DFW", // Dallas/Fort Worth International Airport, USA
                    "DEN", // Denver International Airport, USA
                    "JFK", // John F. Kennedy International Airport, USA
                    "SFO", // San Francisco International Airport, USA
                    "YYZ", // Toronto Pearson International Airport, Canada
                    "YVR", // Vancouver International Airport, Canada
                    "MEX", // Mexico City International Airport, Mexico

                    // Europe
                    "LHR", // London Heathrow Airport, UK
                    "CDG", // Charles de Gaulle Airport, France
                    "FRA", // Frankfurt Airport, Germany
                    "AMS", // Amsterdam Schiphol Airport, Netherlands
                    "MAD", // Adolfo Suárez Madrid-Barajas Airport, Spain
                    "FCO", // Leonardo da Vinci–Fiumicino Airport, Italy
                    "IST", // Istanbul Airport, Turkey
                    "SVO", // Sheremetyevo International Airport, Russia
                    "ZRH", // Zurich Airport, Switzerland
                    "VIE", // Vienna International Airport, Austria

                    // Asia
                    "PEK", // Beijing Capital International Airport, China
                    "PVG", // Shanghai Pudong International Airport, China
                    "HND", // Tokyo Haneda Airport, Japan
                    "NRT", // Narita International Airport, Japan
                    "ICN", // Incheon International Airport, South Korea
                    "SIN", // Singapore Changi Airport, Singapore
                    "HKG", // Hong Kong International Airport, Hong Kong
                    "BKK", // Suvarnabhumi Airport, Thailand
                    "DEL", // Indira Gandhi International Airport, India
                    "DXB", // Dubai International Airport, UAE

                    // Other Regions
                    "SYD", // Sydney Kingsford Smith Airport, Australia
                    "MEL", // Melbourne Airport, Australia
                    "GRU", // São Paulo/Guarulhos–Governador André Franco Montoro International Airport, Brazil
                    "EZE", // Ministro Pistarini International Airport (Ezeiza), Argentina
                    "JNB", // O.R. Tambo International Airport, South Africa
                    "CAI", // Cairo International Airport, Egypt
                };

            int index = random.Next(airports.Count);
            return airports[index];
        }
    }
}