using System;
using System.Collections.Generic;
using System.Threading;
using DF2023.Core.Constants;
using DF2023.WebPageModel;
using Nest;
using Newtonsoft.Json.Linq;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Services;

namespace DF2023.WebPageHelper
{
    public static class PanelHelper
    {

        public static void Create8TousandsDelegation(string baseUrl, string token)
        {
            var endpoint = $"{baseUrl}graphqllayer/GraphQLMutation/Mutation";
            var countries = GetDataHelper.GetCountries(endpoint);
            var services = GetDataHelper.GetServicesLevel(endpoint);
            var enities = GetDataHelper.GetEntities(endpoint);

            List<JObject> list = new List<JObject>();
            for (int i = 0;i<=10;i++)
            {
                var rdCountry = new Random().Next(0,countries.Count -1);
                var rdService = new Random().Next(0, services.Count - 1);
                var rdEntity = new Random().Next(0, enities.Count - 1);
                var delagationModel = GenerateDelegationModel(enities[rdEntity].ToString(), services[rdService].ToString(), countries[rdCountry].ToString());
                var returnedDelegation = CreateNewDelegation(endpoint, token, delagationModel);
                if(returnedDelegation !=null)
                    list.Add(returnedDelegation);
            }

            var countDelegation = list.Count;
        }

        private static DelegationModel GenerateDelegationModel(String entity, String serviceLevel, string country)
        {
            string Title = GenerateName(new Random().Next(1, 20));
            string ContactPhoneNumber = GenerateNumberAsString(new Random().Next(7, 10));
            string ContactEmail = $"{Title}@email.com";
            string NumberOfOfficialDelegates = new Random().Next(1, 10).ToString();
            string TitleAr = $"{Title} - Ar";
            string RemainingNumberOfOfficialDelegates = new Random().Next(1, 5).ToString();
            string IsSingle = new Random().Next(0, 1) == 0 ? "True" : "False";
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
                ServicesLevel = serviceLevel
            };
            return model;
        }

        private static JObject CreateNewDelegation(string endpoint,string token, DelegationModel model)
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
                                systemParentId: ""2a38b7cd-b193-41e7-a050-3238fecfe07b""
                              }}) {{
                                id,
                                title
                              }}
                            }}";
            var delegation = GraphQLHelper.ExecuteQueryAsync(endpoint, query, null);
            return delegation;
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