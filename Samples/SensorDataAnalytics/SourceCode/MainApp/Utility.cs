using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net;
using System.IO;
using PowerBI;

namespace TISensorToEH
{
    public static class Utility
    {
        private static string authority = "https://login.windows.net/common/oauth2/authorize";

        //Power BI resource uri
        private static string resourceUri = "https://analysis.windows.net/powerbi/api";

        //Client app ID 
        private static string clientID = "{Client ID}";

        private static string redirectUri = "https://powerbi.com";



        public static void DeletePBIData()
        {
            //OAuth2 authority

            //Create a new **AuthenticationContext** passing an Authority.
            AuthenticationContext authContext = new AuthenticationContext(authority);

            //Get an Azure Active Directory token by calling **AcquireToken**
            string token = authContext.AcquireToken(resourceUri, clientID, new Uri(redirectUri)).AccessToken.ToString();

            HttpWebRequest request = System.Net.WebRequest.Create(datasetsUri) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = "GET";
            request.ContentLength = 0;
            request.ContentType = "application/json";

            //Pass token in request header
            request.Headers.Add("Authorization", String.Format("Bearer {0}", token));
        
            var baseaddress = new Uri("https://api.powerbi.com/beta/myorg/");

            //using (var httpClient = new httpClient )

            //List<Object> datasets = null;

            //Create a GET web request to list all datasets
            HttpWebRequest request1 = DatsetRequest(datasetsUri, "GET", token);

            //Get HttpWebResponse from GET request
            string responseContent = GetResponse(request1);

            //Get list from response. ToObject() is a sample Power BI extension method
            //List<Object> datasets = responseContent.ToList<Object>();// <Object>(); // <List<Object>>();
            //responseContent.ToList<List<Object>>();
            //responseContent.ToList<Object>();
            //return datasets;

        }

        private static string datasetsUri = "https://api.powerbi.com/beta/myorg/datasets";

        private static HttpWebRequest DatsetRequest(string datasetsUri, string method, string authorizationToken)
        {
            HttpWebRequest request = System.Net.WebRequest.Create(datasetsUri) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = method;
            request.ContentLength = 0;
            request.ContentType = "application/json";

            //Add an Authorization header to an HttpWebRequest object
            request.Headers.Add("Authorization", String.Format("Bearer {0}", authorizationToken));

            return request;
        }


        private static string GetResponse(HttpWebRequest request)
        {
            string response = string.Empty;

            using (HttpWebResponse httpResponse = request.GetResponse() as System.Net.HttpWebResponse)
            {
                //Get StreamReader that holds the response stream
                using (StreamReader reader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                {
                    response = reader.ReadToEnd();
                }
            }

            return response;
        }

        private static void PostRequest(HttpWebRequest request, string json)
        {
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(json);
            request.ContentLength = byteArray.Length;

            //Write JSON byte[] into a Stream
            using (Stream writer = request.GetRequestStream())
            {
                writer.Write(byteArray, 0, byteArray.Length);
            }
        }
        
    }
}
