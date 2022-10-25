using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DataGenerator
{
    public class DataGeneratorFunction
    {
        [FunctionName("DataGeneratorFunction")]
        public async Task Run([TimerTrigger("0 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            int eventsPerMinute = int.Parse(Environment.GetEnvironmentVariable("eventsPerMinute", EnvironmentVariableTarget.Process));
            string ehConn = Environment.GetEnvironmentVariable("eventHubConnectionString", EnvironmentVariableTarget.Process);
            string eh = "click-stream-events";
            await using (var producerClient = new EventHubProducerClient(ehConn, eh))
            {
                var events = Enumerable.Range(0, eventsPerMinute)
                    .Select(i => new ClickStreamEvent())
                    .Select(traffic => JsonConvert.SerializeObject(traffic))
                    .Select(jsonString => Encoding.UTF8.GetBytes(jsonString))
                    .Select(jsonBytes => new EventData(jsonBytes));
                await producerClient.SendAsync(events);
            }
        }
    }

    class ClickStreamEvent
    {
        private readonly static string[] Browsers = { "Edge", "Chrome", "Safari", "Firefox" };
        private static string RandomIp()
        {
            return $"{Random.Shared.Next(0, 255)}.{Random.Shared.Next(0, 255)}.{Random.Shared.Next(0, 255)}.{Random.Shared.Next(0, 255)}";
        }

        public string EventTime { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss");
        public int UserId { get; set; } = Random.Shared.Next(1000);
        public string IP { get; set; } = RandomIp();
        public Request Request { get; set; } = new Request();
        public Response Response { get; set; } = new Response();
        public string Browser { get; set; } = Browsers[Random.Shared.Next(Browsers.Length)];
    }

    class Request
    {
        private readonly static string[] Methods = { "GET", "POST", "PUT", "DELETE" };
        private readonly static string[] URIs = { "/index.html", "/site/user_status.html", "/site/news.html", "/site/shop.html", "/site/about_me.html" };

        public string Method { get; set; } = Methods[Random.Shared.Next(Methods.Length)];
        public string URI { get; set; } = URIs[Random.Shared.Next(URIs.Length)];
        public string Protocol { get; set; } = "HTTP/1.1";
    }

    class Response
    {
        private readonly static int[] Codes = { 200, 403, 404, 500 };

        public int Code { get; set; } = Codes[Random.Shared.Next(Codes.Length)];
        public int Bytes { get; set; } = Random.Shared.Next(10000, 50000);
    }
}
