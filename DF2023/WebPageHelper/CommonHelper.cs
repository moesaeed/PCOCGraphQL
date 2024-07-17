using System.Web;

namespace DF2023.WebPageHelper
{
    public static class CommonHelper
    {
        public static string GetBaseUrl()
        {
            var requestUrl = HttpContext.Current?.Request?.Url;
            string baseUrl = $"{requestUrl.Scheme}://{requestUrl.Host}:{requestUrl.Port}/";
            return baseUrl;
        }

        public static string GetAuthenticatedUserAccessToken()
        {
            //var token = "Yjk3Mzg4OGEtMWRhYy00ZDU2LWJlMmMtZTZkZjc2NDJkMGZlLT1wcm92aWRlcj0tRGVmYXVsdC09c2VjcmV0a2V5PS1ZeT1YYm1UUV5LdTorbnRhQ3Z7PlB3SHR4bHRYJVFoTTtTbUpXYkA+cFBTRVMhVS1uUjRbU01NRUgvaFJOJlpYcFNuUXt2dEJ9dUZNXTtNYX10VmJZXl4yRCRKakxJOXdHRF9IQDBOd19JWlE9eU1qSCMoP0RiRl93QnBlbUttPQ==";
            var token = "Yjk3Mzg4OGEtMWRhYy00ZDU2LWJlMmMtZTZkZjc2NDJkMGZlLT1wcm92aWRlcj0tRGVmYXVsdC09c2VjcmV0a2V5PS1XODsldnNdYnRaUS1DMTBnQkAzd1NAaGFXNGVtRWhnQHJRV1Iwblp2JFJYMHpwZzoxY1kpNFJtIXtLe2VnfW1PTW1hXmEoKiEyaDZJPV1yS1BjOXxTXl1Db15jR0xaeEgwNng/WCEqP0AkZkwhVUItcXFKYjstZ043UXBpRj9SXg==";
            return token;
        }

    }
}