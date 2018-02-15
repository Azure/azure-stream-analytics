using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace CheckMalformedEvents
{
    class MalformedEvents
    {
        //static string connectionString = "HostName=xxxxx.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=xxxxxx";
        static string connectionString;

        private static string partitionId;
        static string iotHubD2cEndpoint = "messages/events";
        static EventHubClient eventHubClient;

        static void Main(string[] args)
        {
            Console.Write("Enter the connection string:");
            connectionString = Console.ReadLine();
            Console.Write("Enter the partition Id:");
            partitionId = Console.ReadLine();
            Console.Write("Enter the resource number:");
            long offset;
            if (long.TryParse(Console.ReadLine(), out offset) == false)
            {
                Console.Write("Enter a valid number.");
                Console.ReadLine();
                return;
            }

            eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);

            PrintMessages(partitionId, offset, 1000);
            Console.WriteLine("Done");
            Console.ReadLine();
        }

        static void PrintMessages(string partitionId, long offset, int numberOfEvents)
        {
            EventHubReceiver receiver;
            try
            {
                receiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partitionId, offset.ToString(), true);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("No data for the specified offset in this partition.");
                return;
            }

            try
            {
                foreach (var e in receiver.Receive(numberOfEvents, TimeSpan.FromMinutes(1)))
                {
                    Console.WriteLine(Encoding.UTF8.GetString(e.GetBytes()));
                    Console.WriteLine("----");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }

            receiver.Close();
        }
    }

}
