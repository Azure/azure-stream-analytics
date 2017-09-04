using System;
using System.Globalization;
using System.Text;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using TollApp.Configs;
using TollApp.Events;

namespace TollApp.Senders
{
    /// <summary>
    /// Creates the event hubs and sends data to the event hubs
    /// </summary>
    public class EventHubSender
    {
        #region Private Variables

        private readonly EventHubClient _entryEventHub;
        private readonly EventHubClient _exitEventHub;

        #endregion

        #region Construcor

        public EventHubSender()
        {
            // create Event Hub
            var eventHubConnectionString = CloudConfiguration.EventHubConnectionString;

            var manager = NamespaceManager.CreateFromConnectionString(eventHubConnectionString);
            manager.CreateEventHubIfNotExistsAsync(CloudConfiguration.EntryName);
            manager.CreateEventHubIfNotExistsAsync(CloudConfiguration.ExitName);

            _entryEventHub = EventHubClient.CreateFromConnectionString(eventHubConnectionString, CloudConfiguration.EntryName);
            _exitEventHub = EventHubClient.CreateFromConnectionString(eventHubConnectionString, CloudConfiguration.ExitName);
        }

        #endregion

        #region Public Methods

        public void SendData(TollEvent data)
        {
            var eventHubName = data is EntryEvent ? _entryEventHub : _exitEventHub;

            try
            {
                eventHubName.Send(new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)))
                {
                    PartitionKey = data.TollId.ToString(CultureInfo.InvariantCulture)
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        public void DisposeSender()
        {
            _entryEventHub.Close();
            _exitEventHub.Close();
        }

        #endregion
    }
}