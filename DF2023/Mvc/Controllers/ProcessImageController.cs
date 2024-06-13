using DF2023.Mvc.Models;
using IdentityModel.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
            try
            {
                string accessToken = GetAccessToken();
                var faceDetectionURL = $"{baseURL}/api/ProcessImage/ProcessFaceDetection";
                var responseBody = SendFaceDetectionRequest(imageDto, accessToken, faceDetectionURL);
                apiResult = JsonConvert.DeserializeObject<ApiResult>(responseBody);
            }
            catch (Exception ex)
            {
                apiResult = new ApiResult(ex.Message, false, null);
            }

            return this.Ok(apiResult);
        }

        [HttpPost]
        public IHttpActionResult ProcessImagePassport(ProcessingImageDto imageDto)
        {
            try
            {
                string accessToken = GetAccessToken();
                var faceDetectionURL = $"{baseURL}/api/ProcessImage/ProcessImagePassport";
                var responseBody = SendFaceDetectionRequest(imageDto, accessToken, faceDetectionURL);

                string dataString = JObject.Parse(responseBody)["Data"].ToString(); // Extract the "Data" part

                PassportImageResult result = JsonConvert.DeserializeObject<PassportImageResult>(dataString);

                return this.Ok(result);
            }
            catch (Exception ex)
            {
                return this.Ok();
            }
        }

        private string SendFaceDetectionRequest(ProcessingImageDto imageDto, string accessToken, string url)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var content = new StringContent(JsonConvert.SerializeObject(imageDto), Encoding.UTF8, "application/json");

                var response = client.PostAsync(url, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = response.Content?.ReadAsStringAsync().Result;
                    return responseBody;
                }
            }

            return null;
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