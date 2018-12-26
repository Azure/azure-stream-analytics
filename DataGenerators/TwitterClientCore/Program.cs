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
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;

namespace TwitterClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //Configure Twitter OAuth
            var oauthToken = ConfigurationManager.AppSettings["oauth_token"];
            var oauthTokenSecret = ConfigurationManager.AppSettings["oauth_token_secret"];
            var oauthCustomerKey = ConfigurationManager.AppSettings["oauth_consumer_key"];
            var oauthConsumerSecret = ConfigurationManager.AppSettings["oauth_consumer_secret"];
            var keywords = ConfigurationManager.AppSettings["twitter_keywords"];

            //Configure EventHub
            var ehConnectionBuilder = new EventHubsConnectionStringBuilder(ConfigurationManager.AppSettings["EventHubConnectionString"])
            {
                EntityPath = ConfigurationManager.AppSettings["EventHubName"]
            };

            EventHubClient client = EventHubClient.CreateFromConnectionString(ehConnectionBuilder.ToString());
            Console.WriteLine($"Sending data eventhub : {client.EventHubName} PartitionCount = {client.GetRuntimeInformationAsync().Result.PartitionCount}");
            
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
            .Select(e => client.SendAsync(e))
            .Buffer(TimeSpan.FromMinutes(1), maxRequestsInProgress)
            .Select(sendTaskList => Task.WhenAll(sendTaskList));
            
            var subscription = sendTasks.Subscribe(
                sendEventDatasTask => sendEventDatasTask.Wait(),
                e => Console.WriteLine(e));
        }
    }
}
