using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using TollApp.Configs;
using TollApp.Events;
using TollApp.Models;
using TollApp.Senders;
using TollApp.Utils;

namespace TollApp
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    public class Program
    {
        #region Private variables 

        private static Timer _timer;
        #endregion

        #region Public Methods

        public static void Main(string[] args)
        {
            JobHost host = new JobHost();
            host.Call(typeof(Program).GetMethod("SendData"));
        }

        [NoAutomaticTrigger]
        public static void SendData()
        {
            var commercialVehicleRegistration = AzureResourcesCreator.CreateBlob();
            AzureResourcesCreator.CreateAzureCosmosDb();
            var eventHubSender = new EventHubSender();

            try
            {
                // generate data
                var generator = TollDataGenerator.Generator(commercialVehicleRegistration);

                var timerInterval = TimeSpan.FromSeconds(Convert.ToDouble(CloudConfiguration.TimerInterval));

                TimerCallback timerCallback = state =>
                {
                    var startTime = DateTime.UtcNow;
                    generator.Next(startTime, timerInterval, 5);

                    foreach (var tollEvent in generator.GetEvents(startTime))
                    {
                        eventHubSender.SendData(tollEvent);
                    }
                    _timer.Change((int)timerInterval.TotalMilliseconds, Timeout.Infinite);
                };

                _timer = new Timer(timerCallback, null, Timeout.Infinite, Timeout.Infinite);
                _timer.Change(0, Timeout.Infinite);

                var exitEvent = new ManualResetEvent(false);
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    Console.WriteLine("Stopping...");
                    eventArgs.Cancel = true;
                    exitEvent.Set();
                };

                exitEvent.WaitOne();
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                Thread.Sleep(timerInterval);
                _timer.Dispose();

                eventHubSender.DisposeSender();
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception.ToString());
            }
        }

        #endregion
    }
}
