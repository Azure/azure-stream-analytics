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


namespace SensorEventGenerator
{
    class EventHubObserver : IObserver<Sensor>
    {
        private EventHubConfig _config;
        private EventHubClient _eventHubClient;
        private Logger _logger;
                
        public EventHubObserver(EventHubConfig config)
        {
            try
            {
                _config = config;
                _eventHubClient = EventHubClient.CreateFromConnectionString(_config.ConnectionString, config.EventHubName);
                this._logger = new Logger(ConfigurationManager.AppSettings["logger_path"]);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                throw ex;
            }

        }
        public void OnNext(Sensor sensorData)
        {
            try
            {

                var serialisedString = JsonConvert.SerializeObject(sensorData);
                EventData data = new EventData(Encoding.UTF8.GetBytes(serialisedString)) { PartitionKey = sensorData.dspl };
                _eventHubClient.Send(data);
               
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Sending" + serialisedString + " at: " + sensorData.time);
                
                //To write every event entry to the logfile, uncomment the line below. 
                //Note: Writing every event can quickly grow the size of the log file.
                //_logger.Write("Sending" + serialisedString + " at: " + sensorData.TimeStamp);

            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                throw ex;
            }

        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            _logger.Write(error);
            throw error;
        }

    }
}
