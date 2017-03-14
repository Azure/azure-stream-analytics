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
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using Microsoft.ServiceBus.Messaging;

    class BrokeredFormatter : IDispatchMessageFormatter
    {
        public void DeserializeRequest(Message message, object[] parameters)
        {
            if (!message.Properties.ContainsKey(BrokeredMessageProperty.Name))
            {
                throw new InvalidOperationException("BrokeredMessageFormatter_BrokeredMessagePropertyMissing");
            }

            BrokeredMessageProperty property = (BrokeredMessageProperty)message.Properties[BrokeredMessageProperty.Name];
            parameters[0] = property.Message;
        }

        // This doesn't matter as this formatter will not be used on the producer side
        public Message SerializeReply(MessageVersion version, object[] parameters, object result)
        {
            throw new NotImplementedException();
        }
    }
}
