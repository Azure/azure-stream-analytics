//---------------------------------------------------------------------------------
// Microsoft (R)  Windows Azure ServiceBus Messaging sample
// 
// Copyright (c) Microsoft Corporation. All rights reserved.  
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
//---------------------------------------------------------------------------------

namespace Microsoft.Samples.ServiceBusMessaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// This is a sample to demonstrate the WCF push based 
    ///     programming to enhance the usability of retrieving 
    ///     messages from a ServiceBus queue or topic
    /// </summary>
    public class Program
    {
        // Azure ServiceBus account specific information
        static string ServiceNamespace;
        static string IssuerName;
        static string IssuerKey;

        static Uri ServiceUri; 
        static string QueueName; 
        static string QueueAddress; 
        static TokenProvider TokenProvider;
        static NamespaceManager NamespaceManager;
       
        /// <summary>
        /// Starts the sample code
        /// </summary>
        public static void Start()
        {
            // Get the credentials for the Azure Service Bus account
            GetUserCredentials();

            ServiceUri = ServiceBusEnvironment.CreateServiceUri("sb", ServiceNamespace, string.Empty);
            
            QueueName = "AzureServiceBusSample_" + Environment.MachineName.ToLower();
            QueueAddress = ServiceUri + QueueName;
            
            TokenProvider = TokenProvider.CreateSharedSecretTokenProvider(IssuerName, IssuerKey);
            NamespaceManager = new NamespaceManager(ServiceUri, TokenProvider);

            ShowOptions(); 
            string input = Console.ReadLine();

            while (!String.Equals(input,"exit", StringComparison.InvariantCultureIgnoreCase))
            {
                switch (input)
                {
                    case "1":
                        {
                            Console.WriteLine("List queues");
                            EnumQueue();
                            break;
                        }

                    case "2":
                        {
                            Console.WriteLine("List topics");
                            EnumTopics();
                            break;
                        }

                    case "3":
                        {
                            Console.WriteLine("Simple Service Bus Queue");
                            SimpleQueueDemo();
                            break;
                        }

                    case "4":
                        {
                            Console.WriteLine("Simple Service Bus Publish/Subscribe");
                            SimplePublishSubscribeDemo();
                            break;
                        }

                    case "5":
                        {
                            Console.WriteLine("Simple WCF client-server communication relayed via Azure Service Bus");
                            SimpleWcfClientServiceViaServiceBus();
                            break;
                        }

                    case "6":
                        {
                            Console.WriteLine("Handling Service Bus queued messages via a delegate handler");
                            QueuedMessageDelegateHandler();
                            break;
                        }

                    case "7":
                        {
                            Console.WriteLine("Handling Service Bus queued messages via WCF service while demuxing based on the brokered message's Action property");
                            QueuedMessageWcfServiceHandler();
                            break;
                        }

                    case "8":
                        {
                            Console.WriteLine("Handling Service Bus sessionful queued messages via a sessionful WCF service");
                            QueuedMessageSessionfulWcfServiceHandler();
                            break;
                        }

                    case "9":
                        {
                            Console.WriteLine("Handling Publish/Subscibe using WCF service");
                            PublishSubscribeMessageWcfServiceHandler();
                            break;
                        }

                    case "10":
                        {
                            Console.WriteLine("Handling Service Bus queued messages in an async method in WCF service");
                            QueuedMessageAsyncWcfServiceHandler();
                            break;
                        }

                    default:
                        {
                            Console.WriteLine("Invalid input! Please re-enter...");
                            break;
                        }
                    }

                ShowOptions();
                input = Console.ReadLine();
                Console.WriteLine("\n"); 
            }
        }

        # region Helper methods
        /// <summary>
        /// Shows the various sample options
        /// </summary>
        static void ShowOptions()
        {
            Console.WriteLine("\n");
            Console.WriteLine("###########################################################################");
            Console.WriteLine("Choose sample code to run : ");
            Console.WriteLine("1. Listing queues");
            Console.WriteLine("2. Listing topics");
            Console.WriteLine("3. Simple Service Bus Queue");
            Console.WriteLine("4. Simple Service Bus Publish/Subscribe");
            Console.WriteLine("5. Simple WCF client-server communication relayed via Azure Service Bus");
            Console.WriteLine("6. Handling Service Bus queued messages via a delegate handler");
            Console.WriteLine("7. Handling Service Bus queued messages via WCF service while demuxing based on the brokered message's Action property");
            Console.WriteLine("8. Handling Service Bus sessionful queued messages via a sessionful WCF service");
            Console.WriteLine("9. Handling Publish/Subscibe using WCF service");
            Console.WriteLine("10. Handling Service Bus queued messages in an async method in WCF service");
            Console.WriteLine("Enter 'exit' to exit the application");
        }

        /// <summary>
        /// Prompts the user for the ServiceBus credentials. 
        /// </summary>
        static void GetUserCredentials()
        {
            Console.Write("Please provide the service namespace to use: ");
            ServiceNamespace = Console.ReadLine();
            Console.Write("Please provide the issuer name to use: ");
            IssuerName = Console.ReadLine();
            Console.Write("Please provide the issuer key to use: ");
            IssuerKey = Console.ReadLine();
        }

        /// <summary>
        /// Helper method to create a queue with the machine name identifier
        /// </summary>
        /// <returns></returns>
        static QueueDescription CreateQueue()
        {
            DeleteQueue();
            Console.WriteLine("Creating a new queue - '{0}':", QueueName);
            QueueDescription queue = NamespaceManager.CreateQueue(QueueName);
            return queue;
        }

        /// <summary>
        /// Helper method to delete the queue with the machine name identifier
        /// </summary>
        static void DeleteQueue()
        {
            foreach (QueueDescription queue in NamespaceManager.GetQueues())
            {
                if (String.Equals(queue.Path, QueueName, StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("Deleting existing queue - '{0}':", QueueName);
                    NamespaceManager.DeleteQueue(QueueName);
                }
            }
        }

        # endregion 

        # region 1. Listing queues
        static void EnumQueue()
        {
            bool AnyQueueExists = false; 
            foreach (QueueDescription queue in NamespaceManager.GetQueues())
            {
                AnyQueueExists = true; 
                Console.WriteLine(queue.Path);
            }
            if (!AnyQueueExists)
            {
                Console.WriteLine("No queue exists in this namespace"); 
            }
        }
        # endregion

        # region 2. Listing topics
        static void EnumTopics()
        {
            bool AnyTopicExists = false; 
            foreach (TopicDescription topic in NamespaceManager.GetTopics())
            {
                AnyTopicExists = true; 
                Console.WriteLine(topic.Path);
            }
            if (!AnyTopicExists)
            {
                Console.WriteLine("No topic exists in this namespace");
            }
        }
        # endregion 

        # region 3. Simple Service Bus Queue
        static void SimpleQueueDemo()
        {
            // Create a SB queue
            QueueDescription queue = CreateQueue();

            // Create a factory and client to talk to this queue
            MessagingFactory factory = MessagingFactory.Create(ServiceUri, TokenProvider);
            QueueClient client = factory.CreateQueueClient(queue.Path);

            // Send a message to the queue
            string requestMessage = "Hello World!";
            client.Send(new BrokeredMessage(requestMessage));
            Console.WriteLine("Message sent to the queue - '{0}' ", requestMessage);

            // Receive the message from the queue
            BrokeredMessage responseQueueMessage = client.Receive();
            string responseMessage = responseQueueMessage.GetBody<string>();
            Console.WriteLine("Message received from the queue - '{0}' ", responseMessage);

            // Cleanup
            client.Close(); 
            factory.Close(); 
        }
        # endregion 

        # region 4. Simple Service Bus Publish/Subscribe
        static void SimplePublishSubscribeDemo()
        {
            string topicName = "news";
            string subscriptionName = "mySubscription";

            // Delete the topic if it already exists
            if (NamespaceManager.TopicExists(topicName))
            {
                NamespaceManager.DeleteTopic(topicName);
            }

            // Create the topic 
            Console.WriteLine("Creating new topic - '{0}'", topicName);
            TopicDescription topic = NamespaceManager.CreateTopic(topicName);

            // Create a subscription
            Console.WriteLine("Creating new subscription - '{0}'", subscriptionName);
            SubscriptionDescription subs = NamespaceManager.CreateSubscription(topic.Path, subscriptionName);

            // Create a factory to talk to the topic & subscription
            MessagingFactory factory = MessagingFactory.Create(ServiceUri, TokenProvider);

            // Create a topic client to talk to this topic
            TopicClient topicClient = factory.CreateTopicClient(topic.Path);

            // Send a message to the topic
            string requestMessage = "Hello World!";
            Console.WriteLine("Message sent to the topic - '{0}' ", requestMessage);
            topicClient.Send(new BrokeredMessage(requestMessage));

            // Create a subscription client to receive messages
            SubscriptionClient subscriptionClient = factory.CreateSubscriptionClient(topic.Path, subs.Name);

            // Receive the messages from the subscription
            BrokeredMessage responseQueueMessage = subscriptionClient.Receive();
            string responseMessage = responseQueueMessage.GetBody<string>();
            Console.WriteLine("Message received from the subscription - '{0}' ", responseMessage);

            // Cleanup
            subscriptionClient.Close();
            topicClient.Close(); 
            factory.Close(); 
        }
        # endregion 

        # region 5. Simple WCF client-server communication relayed via Azure Service Bus
        static void SimpleWcfClientServiceViaServiceBus()
        {
            NetMessagingBinding binding = new NetMessagingBinding();

            // Start up the Echo WCF service listening via Azure Service Bus
            ServiceHost host = new ServiceHost(typeof(EchoService));
            ServiceEndpoint echoServiceEndpoint
                = host.AddServiceEndpoint(typeof(IEchoService), binding, QueueAddress);
            echoServiceEndpoint.Behaviors.Add(new TransportClientEndpointBehavior(TokenProvider));
            host.Open();
            Console.WriteLine("WCF service up and running, listening on {0}", QueueAddress);

            // Create a WCF channel factory and client to send message to the Azure Service Bus
            ChannelFactory<IEchoService> factory = new ChannelFactory<IEchoService>
                (binding, new EndpointAddress(QueueAddress));
            factory.Endpoint.Behaviors.Add(new TransportClientEndpointBehavior(TokenProvider));
            IEchoService proxy = factory.CreateChannel();

            // Send messages to the service
            string requestMessage = "Hello World";
            proxy.Echo(requestMessage);
            Console.WriteLine("Message sent to the queue - '{0}' ", requestMessage);

            // clean up the client and service
            factory.Close();
            host.Close();
        }

        [ServiceContract]
        interface IEchoService
        {
            [OperationContract(IsOneWay = true)]
            void Echo(string msg);
        }

        class EchoService : IEchoService
        {
            public void Echo(string message)
            {
                Console.WriteLine("Echo from service '{0}' ", message);
            }
        }
        # endregion

        # region 6. Handling Service Bus queued messages via a delegate handler
        static void QueuedMessageDelegateHandler()
        {
            // Create a ServiceBus queue. 
            QueueDescription queue = CreateQueue();

            // Create a factory and client to talk to this queue
            MessagingFactory factory = MessagingFactory.Create(ServiceUri, TokenProvider);
            QueueClient client = factory.CreateQueueClient(queue.Path);

            // Use the extension method to pass in a delegate 
            //  which will handle all messages received by the queue
            client.StartListener(message => 
                Console.WriteLine("Message received from the queue - {0}", message.GetBody<string>()));

            // Send a message to the queue
            string helloMessage = "Hello World!";
            client.Send(new BrokeredMessage(helloMessage));
            Console.WriteLine("Message sent to the queue - '{0}' ", helloMessage);

            // Send another message to the queue
            string byeMessage = "Bye World!";
            client.Send(new BrokeredMessage(byeMessage));
            Console.WriteLine("Message sent to the queue - '{0}' ", byeMessage);

            // clean up the client and service
            client.StopListener(); 
            client.Close();
            factory.Close();
        }
        # endregion 

        # region 7. Handling Service Bus queued messages via WCF service while demuxing based on the brokered message's Action property
        static void QueuedMessageWcfServiceHandler()
        {
            // Create a ServiceBus queue. 
            QueueDescription queue = CreateQueue();

            // Create a factory and client to talk to this queue
            MessagingFactory factory = MessagingFactory.Create(ServiceUri, TokenProvider);
            QueueClient client = factory.CreateQueueClient(queue.Path);

            // Use the extension method to start the WCF service 
            //  which will handle the queued messages based on its Action property
            client.StartListener(typeof(MyActionOrientedEchoService));

            string requestMessage = "Hello World!";
            // Send message to the queue - to the EchoOnce operation
            BrokeredMessage message = new BrokeredMessage(requestMessage);
            message.SetAction("EchoOnce");
            Console.WriteLine("Message '{0}' sent with '{1}' action", requestMessage, "EchoOnce");
            client.Send(message);

            // Send message to the queue - to the EchoTwice operation
            BrokeredMessage anotherMessage = new BrokeredMessage(requestMessage);
            anotherMessage.SetAction("EchoTwice");
            Console.WriteLine("Message '{0}' sent with '{1}' action", requestMessage, "EchoTwice");
            client.Send(anotherMessage);

            // Cleanup
            client.StopListener(); 
            client.Close();
            factory.Close();
        }

        /// <summary>
        /// WCF service with two methods to direct a queue message based on its Action property
        /// </summary>
        class MyActionOrientedEchoService
        {
            [OnMessage(Action = "EchoOnce")]
            public void EchoOnce(BrokeredMessage msg)
            {
                Console.WriteLine("Message received by EchoOnce method");
                string messageBody = msg.GetBody<string>();
                Console.WriteLine(messageBody);
            }

            [OnMessage(Action = "EchoTwice")]
            public void EchoTwice(BrokeredMessage msg)
            {
                Console.WriteLine("Message received by EchoTwice method");
                string messageBody = msg.GetBody<string>();
                Console.WriteLine(messageBody);
                Console.WriteLine(messageBody);
            }
        }
        # endregion 

        # region 8. Handling Service Bus sessionful queued messages via a sessionful WCF service
        static void QueuedMessageSessionfulWcfServiceHandler()
        {
            DeleteQueue();
            // Create a sessionful queue
            QueueDescription queue = NamespaceManager.CreateQueue
                (new QueueDescription(QueueName) { RequiresSession = true });

            // Create a factory and client to talk to this queue
            MessagingFactory factory = MessagingFactory.Create(ServiceUri, TokenProvider);
            QueueClient client = factory.CreateQueueClient(queue.Path);

            // Use the extension method to start up the WCF service 
            // which will process the sessionful queued messages received by the queue
            client.StartListener(typeof(MySessionfulService), requiresSession: true);

            string helloMessage = "Hello World!";
            string byeMessage = "Bye World!";

            // Start sending messages to the queue
            // Session 1
            BrokeredMessage message1 = new BrokeredMessage(helloMessage);
            message1.SessionId = "1";
            client.Send(message1);

            BrokeredMessage message2 = new BrokeredMessage(byeMessage);
            message2.SessionId = "1";
            client.Send(message2);

            // Session 2
            BrokeredMessage message3 = new BrokeredMessage(helloMessage);
            message3.SessionId = "2";
            client.Send(message3);

            BrokeredMessage message4 = new BrokeredMessage(byeMessage);
            message4.SessionId = "2";
            client.Send(message4);

            // Cleanup
            client.Close();
            factory.Close(); 
        }

        class MySessionfulService
        {
            public void OnReceiveMessage(BrokeredMessage msg)
            {
                Console.WriteLine("Received Message - {0}, Received by service - {1}",
                    msg.GetBody<string>(), this.GetHashCode());
            }

            public void OnError(Exception e)
            {
                Console.WriteLine("Error occurred");
                Console.WriteLine(e.Message);
            }
        }
        # endregion 

        # region 9. Handling Publish/Subscibe using WCF service
        static void PublishSubscribeMessageWcfServiceHandler()
        {
            string topicName = "news";
            string subscriptionName = "mySubscription";

            // Delete the topic if it already exists
            if (NamespaceManager.TopicExists(topicName))
            {
                NamespaceManager.DeleteTopic(topicName);
            }

            // Create the topic 
            Console.WriteLine("Creating new topic - '{0}'", topicName);
            TopicDescription topic = NamespaceManager.CreateTopic(topicName);

            // Create a subscription
            Console.WriteLine("Creating new subscription - '{0}'", subscriptionName);
            SubscriptionDescription subs = NamespaceManager.CreateSubscription(topic.Path, subscriptionName);

            // Create a factory to talk to the topic & subscription
            MessagingFactory factory = MessagingFactory.Create(ServiceUri, TokenProvider);

            // Create a topic client to talk to this topic
            TopicClient topicClient = factory.CreateTopicClient(topic.Path);

            // Create a subscription client to retrive messages
            SubscriptionClient subscriptionClient = factory.CreateSubscriptionClient(topic.Path, subs.Name);

            // Start the WCF service which will receive the messages sent to the topic
            subscriptionClient.StartListener(typeof(MyQueuedMessageHandlerService)); 

            // Send a message to the topic
            string requestMessage = "Hello World!";
            Console.WriteLine("Message sent to the topic - '{0}' ", requestMessage);
            topicClient.Send(new BrokeredMessage(requestMessage));

            // Cleanup
            subscriptionClient.StopListener(); 
            subscriptionClient.Close();
            topicClient.Close();
            factory.Close(); 
        }

        class MyQueuedMessageHandlerService
        {
            public void OnReceiveMessage(BrokeredMessage msg)
            {
                Console.WriteLine("Received Message - {0}", msg.GetBody<string>(), this.GetHashCode());
            }
        }
        # endregion

        # region 10. Handling Service Bus queued messages in an async method in WCF service
        private static void QueuedMessageAsyncWcfServiceHandler()
        {
            // Create a ServiceBus queue. 
            QueueDescription queue = CreateQueue();

            // Create a factory and client to talk to this queue
            MessagingFactory factory = MessagingFactory.Create(ServiceUri, TokenProvider);
            QueueClient client = factory.CreateQueueClient(queue.Path);

            // Use the extension method to start the WCF service 
            //  which will handle the queued messages based on its Action property
            client.StartListener(typeof(MyAsyncEchoService));

            // Send message to the queue
            string requestMessage = "Hello World!";
            BrokeredMessage message = new BrokeredMessage("Hello World!");
            client.Send(message);
            Console.WriteLine("Message sent to the queue - '{0}' ", requestMessage);

            // Cleanup
            client.StopListener(); 
            client.Close();
            factory.Close();
        }

        class MyAsyncEchoService
        {
            public Task DoComplexTaskAsync(BrokeredMessage message)
            {
                Console.WriteLine("Async method called");
                return Task.Factory.StartNew(() => 
                { 
                    // Do some complex task
                    Console.WriteLine("Starting complex task at {0}", DateTime.Now.ToString());  
                    Thread.Sleep(5000);
                    Console.WriteLine("Completing complex task at {0}", DateTime.Now.ToString());  
                });
            }
        }
        # endregion 
    }
}