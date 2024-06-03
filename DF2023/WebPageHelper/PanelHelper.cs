using System;
using System.Collections.Generic;
using System.Threading;
using DF2023.WebPageModel;
using Nest;
using Newtonsoft.Json.Linq;

namespace DF2023.WebPageHelper
{
    public static class PanelHelper
    {

        public static void Create8TousandsDelegation(string baseUrl)
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
                var returnedDelegation = CreateNewDelegation(endpoint, delagationModel);
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

        private static JObject CreateNewDelegation(string endpoint,DelegationModel model)
        {
            string query = "mutation\r\n{\r\n  delegationSave(delegationArg:\r\n  {\r\n    title:\"titleDelegation\",\r\n    contactEmail:\"email@email.com\",\r\n    contactPhoneNumber:\"d\",\r\n    numberOfOfficialDelegates:4,\r\n    remainingNumberOfOfficialDelegates:2\r\n    isSingle:true,\r\n    contactName:\"v\",\r\n    secondaryEmail:\"SecEmail@email.com\",\r\n    entity:\r\n    {\r\n      id:\"3e06f29f-8235-494e-8385-f8e4963adb2f\"\r\n    },\r\n    country:\r\n    {\r\n      id:\"f1f0a7a1-ec7c-491b-907e-3ddec351a57a\"\r\n    },\r\n    servicesLevel:\r\n    {\r\n      id:\"8f74d0e0-3b6f-47ef-a402-aedd0dc2cd3c\"\r\n    },\r\n    systemParentId:\"2a38b7cd-b193-41e7-a050-3238fecfe07b\"\r\n  })\r\n  {\r\n    id,\r\n    title\r\n  }\r\n}";
            var delegation  = GraphQLHelper.ExecuteQueryAsync(endpoint, query, null);
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