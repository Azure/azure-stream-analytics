using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace SendDataToEHLibrary
{
    public static class SendDataToEventhub
    {

        public static string serviceNamespace { get; set; }
        public static string hubName { get; set; }
        public static string sharedAccessPolicyName { get; set; }
        public static string sharedAccessKey { get; set; }

        public static string deviceName { get; set; }

        public static string status { get; set; }

        public async static Task<bool> SendMessage(string body)
        {
            
            // Namespace info.
            //var serviceNamespace = "sudhesh";
            //var hubName = "test";
            //var deviceName = "tisensortag";
            //var sharedAccessPolicyName = "all";
            //var sharedAccessKey = "ZcfdxuLADoYv3zQycbpddoelgBvogPxnqM7/dfMdU2k=";


            var resourceUri = String.Format("https://{0}.servicebus.windows.net/{1}/publishers/{2}",
                serviceNamespace, hubName, deviceName);
            var sas = ServiceBusSASAuthentication(sharedAccessPolicyName, sharedAccessKey, resourceUri);

            var uri = new Uri(String.Format("https://{0}.servicebus.windows.net/{1}/publishers/{2}/messages?api-version=2014-05",
                serviceNamespace, hubName, deviceName));

            var webClient = new Windows.Web.Http.HttpClient();
            webClient.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("SharedAccessSignature", sas);
            webClient.DefaultRequestHeaders.TryAppendWithoutValidation("Content-Type", "application/atom+xml;type=entry;charset=utf-8");
            var request = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Post, uri)
            {
                Content = new HttpStringContent(body)
            };
            //request.Headers.Add("Authorization", sas);
            //request.Headers.Add("Content-Type", "application/atom+xml;type=entry;charset=utf-8");

            var nowait = await webClient.SendRequestAsync(request);

            return nowait.IsSuccessStatusCode;
        }

        private static string ServiceBusSASAuthentication(string sharedAccessPolicyName, string sharedAccessKey, string uri)
        {
            var expiry = (int)DateTime.UtcNow.AddMinutes(20)
                .Subtract(new DateTime(1970, 1, 1))
                .TotalSeconds;
            var stringToSign = WebUtility.UrlEncode(uri) + "\n" + expiry.ToString();
            var signature = HmacSha256(sharedAccessKey, stringToSign);
            var token = String.Format("sr={0}&sig={1}&se={2}&skn={3}",
                WebUtility.UrlEncode(uri), WebUtility.UrlEncode(signature),
                expiry, sharedAccessPolicyName);

            return token;
        }

        // Because Windows.Security.Cryptography.Core.MacAlgorithmNames.HmacSha256 doesn't
        // exist in WP8.1 context we need to do another implementation
        public static string HmacSha256(string key, string value)
        {
            var keyStrm = CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8);
            var valueStrm = CryptographicBuffer.ConvertStringToBinary(value, BinaryStringEncoding.Utf8);

            var objMacProv = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha256);
            var hash = objMacProv.CreateHash(keyStrm);
            hash.Append(valueStrm);

            return CryptographicBuffer.EncodeToBase64String(hash.GetValueAndReset());
        }

    }
}
