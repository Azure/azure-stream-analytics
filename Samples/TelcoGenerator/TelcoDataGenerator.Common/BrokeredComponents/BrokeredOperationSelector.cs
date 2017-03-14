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
    using System.ServiceModel.Dispatcher;
    using Microsoft.ServiceBus.Messaging;

    class BrokeredOperationSelector : IDispatchOperationSelector
    {
        Dictionary<string, string> actionMap;

        internal const string Action = "Action";

        internal BrokeredOperationSelector(Dictionary<string, string> actionMap)
        {
            this.actionMap = actionMap;
        }

        public string SelectOperation(ref System.ServiceModel.Channels.Message message)
        {
            BrokeredMessage msg = ((BrokeredMessageProperty)message.Properties[BrokeredMessageProperty.Name]).Message;
            string operationName;
            if (msg.Properties.ContainsKey(Action))
            {
                string actionProperty = msg.Properties[Action] as string;
                if (actionProperty != null)
                {
                    bool found = actionMap.TryGetValue(actionProperty, out operationName);
                    if (found)
                    {
                        return operationName;
                    }
                }
            }

            bool defaultMethodFound = actionMap.TryGetValue(OnMessageAttribute.WildcardAction, out operationName);
            if (defaultMethodFound)
            {
                return operationName;
            }
            throw new Exception("No matching method");
        }
    }
}
