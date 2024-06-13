using DF2023.Mvc.Models;
using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web.Http;

namespace DF2023.Mvc.Controllers
{
    public class ProcessImageController : ApiController
    {
        private string baseURL = "https://app-gm-tst-qc-001.azurewebsites.net";

        [HttpPost]
        public IHttpActionResult ProcessFaceDetection(ProcessingImageDto imageDto)
        {
            ApiResult apiResult = null;

            string accessToken = GetAccessToken();
            apiResult = SendFaceDetectionRequest(imageDto, apiResult, accessToken);

            return this.Ok(apiResult);
        }

        private ApiResult SendFaceDetectionRequest(ProcessingImageDto imageDto, ApiResult apiResult, string accessToken)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken); 

                var content = new StringContent(JsonSerializer.Serialize(imageDto), Encoding.UTF8, "application/json"); 
                var faceDetectionURL = $"{baseURL}/api/ProcessImage/ProcessFaceDetection";
                var response = client.PostAsync(faceDetectionURL, content).Result;
                try
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = response.Content?.ReadAsStringAsync().Result;
                        apiResult = JsonSerializer.Deserialize<ApiResult>(responseBody);
                    }
                }
                catch (Exception ex)
                {
                    apiResult = new ApiResult(ex.Message, false, null);
                }
            }

            return apiResult;
        }

        private string GetAccessToken()
        {
            string TokenEndpoint = $"{baseURL}/sitefinity/oauth/token";
            string ClientId = "FlutterFrontend";
            string ClientSecret = "9F052640-24C8-4D81-83F5-7E0D3999F944";
            TokenClient tokenClient = new TokenClient(TokenEndpoint, ClientId, ClientSecret, AuthenticationStyle.PostValues);
            Dictionary<string, string> AdditionalParameters = new Dictionary<string, string>()
            {
                { "grant_type", "password" }
            };

            TokenResponse tokenResponse = tokenClient.RequestResourceOwnerPasswordAsync("mosaeed@mofa.gov.qa", "P@ssw0rd", null, AdditionalParameters).Result;
            string accessToken = tokenResponse.AccessToken;
            return accessToken;
        }
    }
}