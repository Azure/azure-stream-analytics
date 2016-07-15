using System;
using System.Configuration;
using System.Threading;
using Microsoft.Azure;
using Microsoft.Azure.Management.StreamAnalytics;
using Microsoft.Azure.Management.StreamAnalytics.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;


namespace ManageStreamAnalytics_with_.NET
{
    class Program
    {
        static void Main(string[] args)
        {
            string resourceGroupName = "ASASDK";
            string streamAnalyticsJobName = "SdkSampleJob";
            string streamAnalyticsInputName = "Input";
            string streamAnalyticsOutputName = "Output";
            string streamAnalyticsTransformationName = "Transformation";

            // Get authentication token
            TokenCloudCredentials aadTokenCredentials =
                new TokenCloudCredentials(
                    ConfigurationManager.AppSettings["SubscriptionId"],
                    GetAuthorizationHeader());

            // Create Stream Analytics management client
            StreamAnalyticsManagementClient client = new StreamAnalyticsManagementClient(aadTokenCredentials);

        }

        public static string GetAuthorizationHeader()
        {

            AuthenticationResult result = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var context = new AuthenticationContext(ConfigurationManager.AppSettings["ActiveDirectoryEndpoint"] + ConfigurationManager.AppSettings["ActiveDirectoryTenantId"]);

                    result = context.AcquireToken(
                        resource: ConfigurationManager.AppSettings["WindowsManagementUri"],
                        clientId: ConfigurationManager.AppSettings["AsaClientId"],
                        redirectUri: new Uri(ConfigurationManager.AppSettings["RedirectUri"]),
                        promptBehavior: PromptBehavior.Always);
                }
                catch (Exception threadEx)
                {
                    Console.WriteLine(threadEx.Message);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = "AcquireTokenThread";
            thread.Start();
            thread.Join();

            if (result != null)
            {
                return result.AccessToken;
            }

            throw new InvalidOperationException("Failed to acquire token");
        }
    }
}