//------------------------------------------------------------------------------
// <copyright>
//     Copyright (c) Microsoft Corporation. All Rights Reserved.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using RetryPolicy = Microsoft.Practices.TransientFaultHandling.RetryPolicy;

namespace TollApp
{
    public static class EventHubHelper
    {
        private static readonly RetryPolicy RetryPolicy = new RetryPolicy(
            new EventHubTransientErrorDetectionStrategy(),
            new ExponentialBackoff(
                "EventHubInputAdapter",
                5,
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromMilliseconds(500),
                true));

        public static Task ExecuteAsync(Func<Task> taskFunc)
        {
            return RetryPolicy.ExecuteAsync(taskFunc);
        }

        public static Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> taskFunc)
        {
            return RetryPolicy.ExecuteAsync(taskFunc);
        }

        /// <summary>
        /// Create an EventHub path in a given eventhub namespace
        /// </summary>
        /// <param name="eventHubConnectionString">The connection string used to access the EventHub namespace</param>
        /// <param name="eventHubPath">the name of the EventHub path</param>
        /// <param name="shardCount">the shardCount of the EventHub</param>
        public static void CreateEventHubIfNotExists(string eventHubConnectionString, string eventHubPath, int shardCount = 0)
        {
            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(eventHubConnectionString);
            var evenhubDesc = new EventHubDescription(eventHubPath);

            if (shardCount > 0)
            {
                evenhubDesc.PartitionCount = shardCount;
            }

            RetryPolicy.ExecuteAsync(() => namespaceManager.CreateEventHubIfNotExistsAsync(evenhubDesc)).Wait();
        }

        /// <summary>
        /// Delete all EventHub paths in a given eventhub namespace 
        /// </summary>
        /// <param name="eventHubConnectionString">The connection string used to access the EventHub namespace</param>
        public static void DeleteAllEventHubs(string eventHubConnectionString)
        {
            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(eventHubConnectionString);
            Console.WriteLine("Deleting the EventHub data in EventHub with connection string: '{0}'", eventHubConnectionString);
            foreach (var eventhub in RetryPolicy.ExecuteAsync(namespaceManager.GetEventHubsAsync).Result)
            {
                Console.WriteLine("Deleting EventHub '{0}'", eventhub.Path);
                RetryPolicy.ExecuteAsync(() => namespaceManager.DeleteEventHubAsync(eventhub.Path)).Wait();
            }
        }

        private class EventHubTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
        {
            public bool IsTransient(Exception ex)
            {
                var messagingException = ex as MessagingException;
                if ((messagingException != null && messagingException.IsTransient) || ex is TimeoutException)
                {
                    Console.WriteLine(ex);
                    return true;
                }

                Console.WriteLine(ex);
                return false;
            }
        }
    }
}
