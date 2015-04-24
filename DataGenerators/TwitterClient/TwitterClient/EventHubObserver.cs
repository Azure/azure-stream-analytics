//------------------------------------------------------------------------------ 
// <copyright> 
//     Copyright (c) Microsoft Corporation. All Rights Reserved. 
// </copyright> 
//------------------------------------------------------------------------------ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System.Configuration;


namespace TwitterClient
{
    class EventHubObserver : IObserver<Payload>
    {
        private EventHubConfig _config;
        private EventHubClient _eventHubClient;
       
                
        public EventHubObserver(EventHubConfig config)
        {
            try
            {
                _config = config;
                _eventHubClient = EventHubClient.CreateFromConnectionString(_config.ConnectionString, config.EventHubName);
                
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
                EventData data = new EventData(Encoding.UTF8.GetBytes(serialisedString)) { PartitionKey = TwitterPayloadData.Topic };
                _eventHubClient.Send(data);
               
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Sending" + serialisedString + " at: " + TwitterPayloadData.CreatedAt.ToString() );
                                
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
