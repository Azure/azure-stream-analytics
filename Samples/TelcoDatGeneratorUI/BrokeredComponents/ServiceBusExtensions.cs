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
    using System.Collections.Generic;
    using System.ServiceModel;
    using Microsoft.ServiceBus.Messaging;

    public delegate void OnReceiveMessageCallback(BrokeredMessage message);

    static class ServiceBusExtensions
    {
        static Dictionary<QueueClient, BrokeredServiceHost> registeredQueueHosts = new Dictionary<QueueClient, BrokeredServiceHost>();
        static Dictionary<SubscriptionClient, BrokeredServiceHost> registeredSubscriptionHosts = new Dictionary<SubscriptionClient, BrokeredServiceHost>();

        public static void SetAction(this BrokeredMessage message, string action)
        {
            message.Properties.Add(BrokeredOperationSelector.Action, action);
        }

        public static void StartListener(this QueueClient queueClient, Type serviceType, ReceiveMode receiveMode = ReceiveMode.PeekLock, bool requiresSession = false)
        {
            Uri address = new Uri(queueClient.MessagingFactory.Address, queueClient.Path);
            BrokeredServiceHost host = new BrokeredServiceHost(serviceType, address, queueClient.MessagingFactory.GetSettings().TokenProvider, receiveMode, requiresSession);

            registeredQueueHosts.Add(queueClient, host);
            host.Open();
        }

        public static void StartListener(this QueueClient queueClient, object singletonInstance, ReceiveMode receiveMode = ReceiveMode.PeekLock, bool requiresSession = false)
        {
            Uri address = new Uri(queueClient.MessagingFactory.Address, queueClient.Path);
            BrokeredServiceHost host = new BrokeredServiceHost(singletonInstance, address, queueClient.MessagingFactory.GetSettings().TokenProvider, receiveMode, requiresSession);

            registeredQueueHosts.Add(queueClient, host);
            host.Open();
        }

        public static void StartListener(this QueueClient queueClient, OnReceiveMessageCallback onReceiveMessageCallback, ReceiveMode receiveMode = ReceiveMode.PeekLock, bool requiresSession = false)
        {
            queueClient.StartListener(new OnReceiveMessageCallbackDelegator(onReceiveMessageCallback), receiveMode, requiresSession);
        }

        public static void StartListener(this SubscriptionClient subscriptionClient, Type serviceType, ReceiveMode receiveMode = ReceiveMode.PeekLock, bool requiresSession = false)
        {
            Uri address = new Uri(subscriptionClient.MessagingFactory.Address, subscriptionClient.TopicPath + "/Subscriptions/" + subscriptionClient.Name);
            BrokeredServiceHost host = new BrokeredServiceHost(serviceType, address, subscriptionClient.MessagingFactory.GetSettings().TokenProvider, receiveMode, requiresSession);

            registeredSubscriptionHosts.Add(subscriptionClient, host);
            host.Open();
        }

        public static void StartListener(this SubscriptionClient subscriptionClient, object singletonInstance, ReceiveMode receiveMode = ReceiveMode.PeekLock, bool requiresSession = false)
        {
            Uri address = new Uri(subscriptionClient.MessagingFactory.Address, subscriptionClient.TopicPath + "/Subscriptions/" + subscriptionClient.Name);
            BrokeredServiceHost host = new BrokeredServiceHost(singletonInstance, address, subscriptionClient.MessagingFactory.GetSettings().TokenProvider, receiveMode, requiresSession);

            registeredSubscriptionHosts.Add(subscriptionClient, host);
            host.Open();
        }

        public static void StartListener(this SubscriptionClient subscriptionClient, OnReceiveMessageCallback onReceiveMessageCallback, ReceiveMode receiveMode = ReceiveMode.PeekLock, bool requiresSession = false)
        {
            subscriptionClient.StartListener(new OnReceiveMessageCallbackDelegator(onReceiveMessageCallback), receiveMode, requiresSession);
        }

        public static void StopListener(this QueueClient queueClient)
        {
            BrokeredServiceHost host;
            bool found = registeredQueueHosts.TryGetValue(queueClient, out host);
            if (found)
            {
                host.Close();
            }
            else
            {
                throw new Exception("No handler registered");
            }
        }

        public static void StopListener(this SubscriptionClient subscriptionClient)
        {
            BrokeredServiceHost host;
            bool found = registeredSubscriptionHosts.TryGetValue(subscriptionClient, out host);
            if (found)
            {
                host.Close();
            }
            else
            {
                throw new Exception("No handler registered");
            }
        }

        [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single)]
        class OnReceiveMessageCallbackDelegator
        {
            OnReceiveMessageCallback callback;
            
            public OnReceiveMessageCallbackDelegator(OnReceiveMessageCallback callback)
            {
                this.callback = callback;
            }

            public void OnReceiveMessage(BrokeredMessage message)
            {
                this.callback(message);
            }
        }
    }
}
