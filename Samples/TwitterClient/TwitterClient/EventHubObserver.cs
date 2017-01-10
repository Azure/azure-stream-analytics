//********************************************************* 
// 
//    Copyright (c) Microsoft. All rights reserved. 
//    This code is licensed under the Microsoft Public License. 
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF 
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY 
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR 
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT. 
// 
//*********************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System.Configuration;
using TwitterClient.Common;

namespace TwitterClient
{
    public class EventHubObserver : IObserver<Payload>
    {
        private EventHubConfig _config;
        private EventHubClient _eventHubClient;
        public bool AzureOn { get; set; }
                
        public EventHubObserver(EventHubConfig config, bool azureOn = true)
        {
			AzureOn = azureOn;
            try
            {
				
                _config = config;
				if (AzureOn)
				{
					_eventHubClient = EventHubClient.CreateFromConnectionString(_config.ConnectionString, config.EventHubName);
				}
            }
            catch (Exception ex)
            {
               
            }

        }
        public void OnNext(Payload TwitterPayloadData)
        {
            try
            {
				var serialisedString = JsonConvert.SerializeObject(TwitterPayloadData);
				if (AzureOn)
				{
					EventData data = new EventData(Encoding.UTF8.GetBytes(serialisedString)) { PartitionKey = TwitterPayloadData.Topic };
					_eventHubClient.Send(data);
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("Sending" + serialisedString + " at: " + TwitterPayloadData.CreatedAt.ToString());
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("Faked Sending" + serialisedString + " at: " + TwitterPayloadData.CreatedAt.ToString());
				}
          
                                
            }
            catch (Exception ex)
            {
                
            }

        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            
        }

    }
}
