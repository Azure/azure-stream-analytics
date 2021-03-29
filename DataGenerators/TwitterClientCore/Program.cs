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
using System.Linq;
using System.Configuration;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

namespace TwitterClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Configure Twitter OAuth
            var oauthToken = ConfigurationManager.AppSettings["oauth_token"];
            var oauthTokenSecret = ConfigurationManager.AppSettings["oauth_token_secret"];
            var oauthCustomerKey = ConfigurationManager.AppSettings["oauth_consumer_key"];
            var oauthConsumerSecret = ConfigurationManager.AppSettings["oauth_consumer_secret"];
            var keywords = ConfigurationManager.AppSettings["twitter_keywords"];

            var producer = new EventHubProducerClient(
                ConfigurationManager.AppSettings["EventHubConnectionString"],
                ConfigurationManager.AppSettings["EventHubName"],
                new EventHubProducerClientOptions() {
                }
            );

            Console.WriteLine($"Sending data eventhub : {producer.EventHubName} PartitionCount = {(await producer.GetPartitionIdsAsync()).Count()}");
            
            IObservable<string> twitterStream = TwitterStream.StreamStatuses(
                new TwitterConfig(
                    oauthToken, 
                    oauthTokenSecret, 
                    oauthCustomerKey, 
                    oauthConsumerSecret,
                    keywords))
                    .ToObservable();

            int maxMessageSizeInBytes = 250 * 1024;
            int maxSecondsToBuffer = 20;

            IObservable<EventData> eventDataObserver = Observable.Create<EventData>(
                outputObserver => twitterStream.Subscribe(
                    new EventDataGenerator(outputObserver, maxMessageSizeInBytes, maxSecondsToBuffer)));
            
            // keep upto 5 ongoing requests.
            int maxRequestsInProgress = 5;
            IObservable<Task> sendTasks = eventDataObserver
            .Select(e => 
            {
                var batch = producer.CreateBatchAsync().Result;
                if(!batch.TryAdd(e))
                {
                    throw new ArgumentOutOfRangeException("Content too big to send in a single eventhub message");
                }
                return producer.SendAsync(batch);
            })
            .Buffer(TimeSpan.FromMinutes(1), maxRequestsInProgress)
            .Select(sendTaskList => Task.WhenAll(sendTaskList));
            
            var subscription = sendTasks.Subscribe(
                sendEventDatasTask => sendEventDatasTask.Wait(),
                e => Console.WriteLine(e));
        }
    }
}
