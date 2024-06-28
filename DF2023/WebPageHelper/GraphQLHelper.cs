using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Text;
using System;
using Telerik.Sitefinity.Abstractions;

namespace DF2023.WebPageHelper
{
    public static class GraphQLHelper
    {
        public static JObject ExecuteQueryAsync(string baseUrl, string graphqlQuery, JObject variables, string token = "")
        {
            var serializedData = JsonConvert.SerializeObject(new
            {
                query = graphqlQuery,
                variables = variables
            });

            var payload = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpClient _httpClient = new HttpClient();
            if (!string.IsNullOrWhiteSpace(token))
                _httpClient.DefaultRequestHeaders.Add("X-SF-Access-Key", token);
            string endPoint = baseUrl + "graphqllayer/GraphQLMutation/Mutation";

            using (var response = _httpClient.PostAsync(endPoint, payload).Result)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    try
                    {
                        return JsonConvert.DeserializeObject<JObject>(responseBody);
                    }
                    catch (Exception ex)
                    {
                        Log.Write($"[GQL] Payload {payload} \n Exception {ex.ToString()}");
                        return new JObject(new JProperty("error", responseBody.ToString()));
                    }
                }
                else
                {
                    throw new WebException("Web service error: \n" + response.Content.ReadAsStringAsync().Result + "\nOriginal query: " + serializedData);
                }
            }
        }
    }
}