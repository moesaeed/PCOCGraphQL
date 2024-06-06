using DF2023.WebPageModel;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace DF2023.WebPageHelper
{
    public class GetDataHelper
    {
        public static List<JToken> GetCountries(string baseUrl)
        {
            string query = "query\r\n{\r\ncountry\r\n  {\r\n    countryItems\r\n    {\r\n      id,\r\n      title\r\n    }\r\n  }\r\n}";
            var countries = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var countryItems = countries.SelectToken("data").SelectToken("country").SelectToken("countryItems").Select(x => x["id"]).ToList();
            return countryItems;
        }
        public static List<JToken> GetServicesLevel(string baseUrl)
        {
            string query = "query\r\n{\r\nservicesLevel\r\n  {\r\n    servicesLevelItems\r\n    {\r\n      id,\r\n      title\r\n    }\r\n  }\r\n}";
            var services = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var servicesItems = services.SelectToken("data").SelectToken("servicesLevel").SelectToken("servicesLevelItems").Select(x => x["id"]).ToList();
            return servicesItems;
        }

        public static List<JToken> GetEntities(string baseUrl)
        {
            string query = "query\n{\nentity\n  {\n   entityItems\n    {\n      id,\n      title\n    }\n  }\n}";
            var entities = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var entitiesItems = entities.SelectToken("data").SelectToken("entity").SelectToken("entityItems").Select(x => x["id"]).ToList();
            return entitiesItems;
        }

        public static List<JToken> GetConventions(string baseUrl)
        {
            string query = "query\n{\nconvention\n  {\n   conventionItems\n    {\n      id,\n      title\n    }\n  }\n}";
            var conventions = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var conventionsItems = conventions.SelectToken("data").SelectToken("convention").SelectToken("conventionItems").Select(x => x["id"]).ToList();
            return conventionsItems;
        }

        public static List<ConventionModel> GetConventionsForDropDown(string baseUrl)
        {
            string query = "query\n{\nconvention\n  {\n   conventionItems\n    {\n      id,\n      title\n    }\n  }\n}";
            var conventions = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var conventionsItems = conventions.SelectToken("data").SelectToken("convention").SelectToken("conventionItems").ToList();
            List<ConventionModel> list = new List<ConventionModel>();
            foreach(var convention in conventionsItems)
            {
                list.Add(new ConventionModel()
                {
                    Id = convention["id"]?.ToString(),
                    Title = convention["title"]?.ToString()
                });
            }
            return list;
        }
    }
}