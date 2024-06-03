using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Text;

namespace DF2023.WebPageHelper
{
    public static class GraphQLHelper
    {
        public static JObject ExecuteQueryAsync(string endPoint, string graphqlQuery, JObject variables)
        {
            var serializedData = JsonConvert.SerializeObject(new
            {
                query = graphqlQuery,
                variables = variables
            });

            var payload = new StringContent(serializedData, Encoding.UTF8, "application/json");

            HttpClient _httpClient = new HttpClient();

            using (var response = _httpClient.PostAsync(endPoint, payload).Result)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<JObject>(responseBody);
                }
                else
                {
                    throw new WebException("Web service error: \n" + response.Content.ReadAsStringAsync().Result + "\nOriginal query: " + serializedData);
                }
            }
        }
    }
}