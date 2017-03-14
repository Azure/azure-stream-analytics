//---------------------------------------------------------------------------------
// Microsoft (R)  Windows Azure SDK
// Software Development Kit
// 
// Copyright (c) Microsoft Corporation. All rights reserved.  
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
//---------------------------------------------------------------------------------

namespace Microsoft.Samples.PublishSubscribe
{
    using System;
    using System.Collections.Generic;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// This is a sample for ServiceBus Publish/Subscribe feature.
    /// </summary>
    public class Program
    {
        const string IssueTrackingTopic = "IssueTrackingTopic";
        const string AuditSubscription = "AuditSubscription";
        const string AgentSubscription = "AgentSubscription";
        static string ServiceNamespace;
        static string IssuerName;
        static string IssuerKey;

        /// <summary>
        /// Start the demo code.
        /// </summary>
        /// <param name="args"></param>
        public static void Start(string[] args)
        {
            GetUserCredentials();
            TokenProvider tokenProvider =
                TokenProvider.CreateSharedSecretTokenProvider(IssuerName, IssuerKey);
            Uri serviceUri = ServiceBusEnvironment.CreateServiceUri("sb", ServiceNamespace, string.Empty);

            //*****************************************************************************************************
            //                                     Management Operations
            //*****************************************************************************************************            

            NamespaceManager namespaceManager = new NamespaceManager(serviceUri, tokenProvider);

            // Create a topic
            Console.WriteLine("\nCreating Topic 'IssueTrackingTopic'...");

            // Delete if exists
            if (namespaceManager.TopicExists(Program.IssueTrackingTopic))
            {
                namespaceManager.DeleteTopic(Program.IssueTrackingTopic);
            }

            TopicDescription myTopic = namespaceManager.CreateTopic(Program.IssueTrackingTopic);

            // Create a subscription
            Console.WriteLine("Creating Subscriptions 'AuditSubscription' and 'AgentSubscription'...");
            SubscriptionDescription myAuditSubscription = namespaceManager.CreateSubscription(Program.IssueTrackingTopic, Program.AuditSubscription);
            SubscriptionDescription myAgentSubscription = namespaceManager.CreateSubscription(Program.IssueTrackingTopic, Program.AgentSubscription);

            //*****************************************************************************************************
            //                                   Runtime Operations
            //*****************************************************************************************************

            MessagingFactory factory = MessagingFactory.Create(serviceUri, tokenProvider);
            try
            {
                Program.Send(factory);
                Program.Receive(factory);
            }
            finally
            {
                factory.Close();
            }

            Console.WriteLine("\nEnd of scenario, press ENTER to clean up and exit.");
            Console.ReadLine();
            namespaceManager.DeleteTopic(Program.IssueTrackingTopic);
        }

        static void Send(MessagingFactory factory)
        {
            TopicClient myTopicClient = factory.CreateTopicClient(Program.IssueTrackingTopic);

            //*****************************************************************************************************
            //                                   Sending messages to a Topic
            //*****************************************************************************************************

            List<BrokeredMessage> messageList = new List<BrokeredMessage>();
            messageList.Add(CreateIssueMessage("1", "First message information"));
            messageList.Add(CreateIssueMessage("2", "Second message information"));
            messageList.Add(CreateIssueMessage("3", "Third message information"));

            Console.WriteLine("\nSending messages to topic...");

            foreach (BrokeredMessage message in messageList)
            {
                myTopicClient.Send(message);
                Console.WriteLine(
                    string.Format("Message sent: Id = {0}, Body = {1}", message.MessageId, message.GetBody<string>()));
            }
        }

        static void Receive(MessagingFactory factory)
        {
            SubscriptionClient agentSubscriptionClient = factory.CreateSubscriptionClient(Program.IssueTrackingTopic, Program.AgentSubscription, ReceiveMode.PeekLock);
            SubscriptionClient auditSubscriptionClient = factory.CreateSubscriptionClient(Program.IssueTrackingTopic, Program.AuditSubscription, ReceiveMode.ReceiveAndDelete);

            //*****************************************************************************************************
            //                                   Receiving messages from a Subscription
            //*****************************************************************************************************

            BrokeredMessage message;
            Console.WriteLine("\nReceiving message from AgentSubscription...");
            while ((message = agentSubscriptionClient.Receive(TimeSpan.FromSeconds(5))) != null)
            {
                Console.WriteLine(string.Format("Message received: Id = {0}, Body = {1}", message.MessageId, message.GetBody<string>()));

                // Further custom message processing could go here...
                message.Complete();
            }

            // Create a receiver using ReceiveAndDelete mode
            Console.WriteLine("\nReceiving message from AuditSubscription...");
            while ((message = auditSubscriptionClient.Receive(TimeSpan.FromSeconds(5))) != null)
            {
                Console.WriteLine(string.Format("Message received: Id = {0}, Body = {1}", message.MessageId, message.GetBody<string>()));

                // Further custom message processing could go here...
            }
        }

        static void GetUserCredentials()
        {
            Console.Write("Please provide the service namespace to use: ");
            ServiceNamespace = Console.ReadLine();
            Console.Write("Please provide the issuer name to use: ");
            IssuerName = Console.ReadLine();
            Console.Write("Please provide the issuer key to use: ");
            IssuerKey = Console.ReadLine();
        }

        static BrokeredMessage CreateIssueMessage(string issueId, string issueBody)
        {
            BrokeredMessage message = new BrokeredMessage(issueBody)
            {
                MessageId = issueId
            };

            return message;
        }
    }
}
