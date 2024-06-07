using System;
using System.Collections.Generic;
using System.Threading;
using DF2023.WebPageModel;
using Newtonsoft.Json.Linq;

namespace DF2023.WebPageHelper
{
    public static class PanelHelper
    {

        public static void CreateDelegation(string baseUrl,int numberOfDelegationToCreate, string parentId, string token)
        {
            //var endpoint = $"{baseUrl}graphqllayer/GraphQLMutation/Mutation";
            var countries = GetDataHelper.GetCountries(baseUrl);
            var services = GetDataHelper.GetServicesLevel(baseUrl);
            var enities = GetDataHelper.GetEntities(baseUrl);
            
            List<JObject> list = new List<JObject>();
            for (int i = 0;i< numberOfDelegationToCreate; i++)
            {
                var rdCountry = new Random().Next(0,countries.Count -1);
                var rdService = new Random().Next(0, services.Count - 1);
                var rdEntity = new Random().Next(0, enities.Count - 1);
                var delagationModel = GenerateDelegationModel(parentId,enities[rdEntity].ToString(), services[rdService].ToString(), countries[rdCountry].ToString());
                var returnedDelegation = CreateNewDelegation(baseUrl, token, delagationModel);
                if(returnedDelegation !=null)
                    list.Add(returnedDelegation);
            }

            var countDelegation = list.Count;
        }

        private static DelegationModel GenerateDelegationModel(string parentId, string entity, string serviceLevel, string country)
        {
            string Title = GenerateName(new Random().Next(1, 20));
            string ContactPhoneNumber = GenerateNumberAsString(new Random().Next(7, 10));
            string ContactEmail = $"{Title}@email.com";
            string NumberOfOfficialDelegates = new Random().Next(1, 10).ToString();
            string TitleAr = $"{Title} - Ar";
            string RemainingNumberOfOfficialDelegates = new Random().Next(1, 5).ToString();
            string IsSingle = new Random().Next(0, 1) == 0 ? "true" : "false";
            string ContactName = GenerateName(new Random().Next(1, 20));
            string SecondaryEmail = $"{ContactName}@email.com";
            DelegationModel model = new DelegationModel()
            {
                Title = Title,
                ContactPhoneNumber = ContactPhoneNumber,
                ContactEmail = ContactEmail,
                NumberOfOfficialDelegates = NumberOfOfficialDelegates,
                TitleAr = TitleAr,
                RemainingNumberOfOfficialDelegates = RemainingNumberOfOfficialDelegates,
                IsSingle = IsSingle,
                ContactName = ContactName,
                SecondaryEmail = SecondaryEmail,
                Country = country,
                Entity = entity,
                ServicesLevel = serviceLevel,
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
                                title
                              }}
                            }}";
            var delegation = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null,token);
            return delegation;
        }

        public static void CreateGuest(string baseUrl, int numberOfGuestsToCreate, string parentId, string token)
        {
            List<JObject> list = new List<JObject>();
            for (int i = 0; i < numberOfGuestsToCreate; i++)
            {
                var guestModel = GenerateGuestModel(parentId);
                var returnedGuest = CreateNewGuest(baseUrl, token, guestModel);
                if (returnedGuest != null)
                    list.Add(returnedGuest);
            }
            var countGuest = list.Count;
        }

        private static JObject CreateNewGuest(string baseUrl, string token, GuestModel model)
        {
            string query = $@"mutation {{
                              guestSave(guestArg: {{
                                title: ""{model.Title}"",
                                systemParentId: ""{model.SystemParentId}""
                              }}) {{
                                id,
                                title
                              }}
                            }}";
            var guest = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null, token);
            return guest;
        }

        private static GuestModel GenerateGuestModel(string parentId)
        {
            string Title = GenerateName(new Random().Next(1, 20));
            string JobTitle = GenerateName(new Random().Next(1, 20));
            string PersonTitle = GenerateName(new Random().Next(1, 20));
            string FirstName = GenerateName(new Random().Next(1, 15));
            string FirstNameAr = GenerateArabicName(new Random().Next(1, 10));
            string SecondName = GenerateName(new Random().Next(1, 15));
            string SecondNameAr = GenerateArabicName(new Random().Next(1, 10));
            string ThirdName = GenerateName(new Random().Next(1, 15));
            string ThirdNameAr = GenerateArabicName(new Random().Next(1, 10));
            string FourthName = GenerateName(new Random().Next(1, 15));
            string FourthNameAr = GenerateArabicName(new Random().Next(1, 10));
            string Email = $"{Title}@email.com";
            string SecondaryEmail = $"{Title}_2@email.com";
            GuestModel model = new GuestModel()
            {
                Title = Title,
                JobTitle = JobTitle,
                PersonTitle = PersonTitle,
                FirstName=FirstName,
                FirstNameAr=FirstNameAr,
                SecondName = SecondName,
                SecondNameAr = SecondNameAr,
                ThirdName = ThirdName,
                ThirdNameAr = ThirdNameAr,
                FourthName = FourthName,
                FourthNameAr =FourthNameAr,
                Email =Email,
                SecondaryEmail = SecondaryEmail      
            };
            return model;
        }

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
    }
}