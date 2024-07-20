using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using Telerik.Sitefinity.Authentication.Configuration;
using Telerik.Sitefinity.Configuration;

namespace DF2023.Core.Helpers
{
    public static class TokenHelper
    {
        public static Tokenobject GetToken(string url, string username, string password)
        {
            //var collection = new List<KeyValuePair<string, string>>();
            //collection.Add(new KeyValuePair<string, string>("username", username));
            //collection.Add(new KeyValuePair<string, string>("password", password));
            //collection.Add(new KeyValuePair<string, string>("grant_type", "password"));
            //collection.Add(new KeyValuePair<string, string>("client_id", "Flutter"));
            //collection.Add(new KeyValuePair<string, string>("client_secret", "BA17CECE-81F6-4350-8AB6-08E54D9554E4"));

            string client_id = string.Empty;
            string client_secret = string.Empty;

            var authenticationConfig = Config.Get<AuthenticationConfig>();
            var authorizedClients = authenticationConfig.OAuthServer.AuthorizedClients.Values;

            if (authorizedClients != null && authorizedClients.Count == 1)
            {
                foreach (var authorizedClient in authorizedClients)
                {
                    client_id = authorizedClient.ClientId;
                    client_secret = authorizedClient.Secret;
                }
            }

            var formData = new Dictionary<string, string>()
            {
                { "username", username },
                { "password", password },
                { "grant_type", "password" },
                { "client_id",client_id},
                {"client_secret",client_secret }
            };

            string headers = string.Empty;

            using (HttpClient c = new HttpClient())
            {
                c.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

                var sc = new FormUrlEncodedContent(formData);
                url += "/sitefinity/oauth/token";

                var result = c.PostAsync(url, sc).Result;
                headers = result.RequestMessage.Headers.ToString();

                if (!result.IsSuccessStatusCode)
                {
                    string responseBody = string.Empty;

                    try
                    {
                        responseBody = result.Content?.ReadAsStringAsync().Result;
                    }
                    catch (Exception ex)
                    {
                        responseBody = $"Error reading response body: {ex.Message}";
                    }
                    finally
                    {
                        throw new Exception($"Error {result.StatusCode} \n {responseBody} ");
                    }
                }
                else
                {
                    var responseBody = result.Content.ReadAsStringAsync().Result;
                    var token = JsonSerializer.Deserialize<Tokenobject>(responseBody);

                    return token;
                }
            }
        }
    }

    public class Tokenobject
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
    }
}