using DF2023.WebPageModel;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace DF2023.WebPageHelper
{
    public class GetDataHelper
    {
        public static List<JToken> GetData(string baseUrl, string content)
        {
            string query = $"query\r\n{{\r\n{content}\r\n  {{\r\n    {content}Items\r\n    {{\r\n      id,\r\n      title\r\n    }}\r\n  }}\r\n}}";
            var data = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var items = data.SelectToken("data").SelectToken(content).SelectToken($"{content}Items").Select(x => x["id"]).ToList();
            return items;
        }
       
        public static List<ConventionModel> GetConventionsForDropDown(string baseUrl)
        {
            List<ConventionModel> list = new List<ConventionModel>();
            string query = "query\n{\nconvention\n  {\n   conventionItems\n    {\n      id,\n      title\n    }\n  }\n}";
            var conventions = GraphQLHelper.ExecuteQueryAsync(baseUrl, query, null);
            var conventionsItems = conventions.SelectToken("data").SelectToken("convention").SelectToken("conventionItems").ToList();
            
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