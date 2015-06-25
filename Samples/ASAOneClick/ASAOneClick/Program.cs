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
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace SensorEventGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new EventHubConfig();
                  
            
            // Uncomment for picking from Configuration 
            config.ConnectionString = ConfigurationManager.AppSettings["EventHubConnectionString"];
            config.EventHubName = ConfigurationManager.AppSettings["EventHubName"];
                        
            //To push 1 event per second
            var eventHubevents = Observable.Interval(TimeSpan.FromSeconds(.1)).Select(i => Sensor.Generate());

            //To send Data to EventHub as JSON
            var eventHubDis = eventHubevents.Subscribe(new EventHubObserver(config));
                                
            Console.ReadLine();
            eventHubDis.Dispose();
                   
	
        }
    }
}
