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
            var data = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var items = data.SelectToken("data").SelectToken("country").SelectToken("countryItems").Select(x => x["id"]).ToList();
            return items;
        }
        public static List<JToken> GetServicesLevel(string baseUrl)
        {
            string query = "query\r\n{\r\nservicesLevel\r\n  {\r\n    servicesLevelItems\r\n    {\r\n      id,\r\n      title\r\n    }\r\n  }\r\n}";
            var data = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var items = data.SelectToken("data").SelectToken("servicesLevel").SelectToken("servicesLevelItems").Select(x => x["id"]).ToList();
            return items;
        }

        public static List<JToken> GetEntities(string baseUrl)
        {
            string query = "query\n{\nentity\n  {\n   entityItems\n    {\n      id,\n      title\n    }\n  }\n}";
            var data = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var items = data.SelectToken("data").SelectToken("entity").SelectToken("entityItems").Select(x => x["id"]).ToList();
            return items;
        }

        public static List<JToken> GetConventions(string baseUrl)
        {
            string query = "query\n{\nconvention\n  {\n   conventionItems\n    {\n      id,\n      title\n    }\n  }\n}";
            var data = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var items = data.SelectToken("data").SelectToken("convention").SelectToken("conventionItems").Select(x => x["id"]).ToList();
            return items;
        }

        public static List<JToken> GetPassportType(string baseUrl)
        {
            string query = "query\r\n{\r\npassporttype\r\n  {\r\n    passporttypeItems\r\n    {\r\n      id,\r\n      title\r\n    }\r\n  }\r\n}";
            var data = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var items = data.SelectToken("data").SelectToken("passporttype").SelectToken("passporttypeItems").Select(x => x["id"]).ToList();
            return items;
        }
        public static List<JToken> GetDelegationType(string baseUrl)
        {
            string query = "query\r\n{\r\ndelegationType\r\n  {\r\n    delegationTypeItems\r\n    {\r\n      id,\r\n      title\r\n    }\r\n  }\r\n}";
            var data = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var items = data.SelectToken("data").SelectToken("delegationType").SelectToken("delegationTypeItems").Select(x => x["id"]).ToList();
            return items;
        }
        public static List<JToken> GetTitleList(string baseUrl)
        {
            string query = "query\r\n{\r\ntitlelist\r\n  {\r\n    titlelistItems\r\n    {\r\n      id,\r\n      title\r\n    }\r\n  }\r\n}";
            var data = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var items = data.SelectToken("data").SelectToken("titlelist").SelectToken("titlelistItems").Select(x => x["id"]).ToList();
            return items;
        }
        public static List<JToken> GetAirports(string baseUrl)
        {
            string query = "query\r\n{\r\nairport\r\n  {\r\n    airportItems\r\n    {\r\n      id,\r\n      title\r\n    }\r\n  }\r\n}";
            var data = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var items = data.SelectToken("data").SelectToken("airport").SelectToken("airportItems").Select(x => x["id"]).ToList();
            return items;
        }
        public static List<JToken> GetGuestStatus(string baseUrl)
        {
            string query = "query\r\n{\r\ngueststatus\r\n  {\r\n    gueststatusItems\r\n    {\r\n      id,\r\n      title\r\n    }\r\n  }\r\n}";
            var data = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var items = data.SelectToken("data").SelectToken("gueststatus").SelectToken("gueststatusItems").Select(x => x["id"]).ToList();
            return items;
        }
        public static List<JToken> GetGuestStageStatus(string baseUrl)
        {
            string query = "query\r\n{\r\ngueststagestatus\r\n  {\r\n    gueststagestatusItems\r\n    {\r\n      id,\r\n      title\r\n    }\r\n  }\r\n}";
            var data = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var items = data.SelectToken("data").SelectToken("gueststagestatus").SelectToken("gueststagestatusItems").Select(x => x["id"]).ToList();
            return items;
        }
        public static List<JToken> GetSubAttendeeType(string baseUrl)
        {
            string query = "query\r\n{\r\nsubattendeetype\r\n  {\r\n    subattendeetypeItems\r\n    {\r\n      id,\r\n      title\r\n    }\r\n  }\r\n}";
            var data = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var items = data.SelectToken("data").SelectToken("subattendeetype").SelectToken("subattendeetypeItems").Select(x => x["id"]).ToList();
            return items;
        }
        public static List<JToken> GetBadgeType(string baseUrl)
        {
            string query = "query\r\n{\r\nbadgetype\r\n  {\r\n    badgetypeItems\r\n    {\r\n      id,\r\n      title\r\n    }\r\n  }\r\n}";
            var data = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var items = data.SelectToken("data").SelectToken("badgetype").SelectToken("badgetypeItems").Select(x => x["id"]).ToList();
            return items;
        }
        public static List<JToken> GetDelegationMemberType(string baseUrl)
        {
            string query = "query\r\n{\r\ndelegationmembertype\r\n  {\r\n    delegationmembertypeItems\r\n    {\r\n      id,\r\n      title\r\n    }\r\n  }\r\n}";
            var data = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var items = data.SelectToken("data").SelectToken("delegationmembertype").SelectToken("delegationmembertypeItems").Select(x => x["id"]).ToList();
            return items;
        }

        public static List<JToken> GetDelegationByConventionId(string baseUrl, string Id)
        {
            string query = $"query\r\n{{\r\n  delegation(_filter:\r\n  {{\r\n    systemParentId:\"{Id}\"\r\n  }})\r\n  {{\r\n    delegationItems{{\r\n      id,\r\n      title\r\n    }}\r\n  }}\r\n}}";
            var data = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var items = data.SelectToken("data").SelectToken("delegation").SelectToken("delegationItems").Select(x => x["id"]).ToList();
            return items;
        }

        public static List<JToken> GetGuestsByConventionId(string baseUrl, string Id)
        {
            string query = $"query\r\n{{\r\n  guest(_filter:\r\n  {{\r\n    systemParentId:\"{Id}\"\r\n  }})\r\n  {{\r\n    guestItems{{\r\n      id,\r\n      title\r\n    }}\r\n  }}\r\n}}";
            var data = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var items = data.SelectToken("data").SelectToken("guest").SelectToken("guestItems").Select(x => x["id"]).ToList();
            return items;
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