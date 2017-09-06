using System;
using System.IO;
using System.Threading;
using Microsoft.Azure.WebJobs;
using TollApp.Configs;
using TollApp.Events;
using TollApp.Senders;
using TollApp.Utils;

namespace TollApp
{
    /// <summary>
    /// The WebJob's starting point
    /// </summary>
    public class Program
    {
        #region Private variables 

        private static Timer _timer;
        private static TimeSpan _timerInterval;
        private static int _eventCount;

        #endregion

        #region Public Methods

        public static void Main(string[] args)
        {
            JobHost host = new JobHost();
            host.Call(typeof(Program).GetMethod("SendData"));
        }

        [NoAutomaticTrigger]
        public static void SendData(TextWriter log)
        {
            var startTimeSpan = TimeSpan.FromMinutes(1);
            var periodTimeSpan = TimeSpan.FromMinutes(1);

            var commercialVehicleRegistration = AzureResourcesCreator.CreateBlob();
            AzureResourcesCreator.CreateAzureCosmosDb();
            var eventHubSender = new EventHubSender();

            try
            {
                // generate data
                var generator = TollDataGenerator.Generator(commercialVehicleRegistration);

                _timerInterval = TimeSpan.FromSeconds(Convert.ToDouble(CloudConfiguration.TimerInterval));

                TimerCallback timerCallback = state =>
                {
                    var startTime = DateTime.UtcNow;
                    generator.Next(startTime, _timerInterval, 5);

                    foreach (var tollEvent in generator.GetEvents(startTime))
                    {
                        eventHubSender.SendData(tollEvent);
                        ++_eventCount;

                        var logger = new Timer((e) =>
                        {
                            log.WriteLine("Number of events sent: " + _eventCount);
                        }, null, startTimeSpan, periodTimeSpan);

                    }
                    _timer.Change((int) _timerInterval.TotalMilliseconds, Timeout.Infinite);
                };
                _timer = new Timer(timerCallback, null, Timeout.Infinite, Timeout.Infinite);
                _timer.Change(0, Timeout.Infinite);

                log.WriteLine("Sending event hub data");
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception.ToString());
            }
            finally
            {
                var exitEvent = new ManualResetEvent(false);
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    Console.WriteLine("Stopping...");
                    eventArgs.Cancel = true;
                    exitEvent.Set();
                };


                exitEvent.WaitOne();
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                Thread.Sleep(_timerInterval);
                _timer.Dispose();

                eventHubSender.DisposeSender();
            }
        }

        #endregion
    }
}
