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
    using System.ServiceModel.Channels;
    using Microsoft.ServiceBus.Messaging;

    class BrokeredBinding : Binding
    {
        BrokeredEncodingBindingElement encodingElement;
        NetMessagingTransportBindingElement transportElement;

        public BrokeredBinding()
        {
            this.encodingElement = new BrokeredEncodingBindingElement();
            this.transportElement = new NetMessagingTransportBindingElement();
        }

        public override string Scheme 
        { 
            get 
            { 
                return this.transportElement.Scheme; 
            } 
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection elements = new BindingElementCollection();
            elements.Add(this.encodingElement);
            elements.Add(this.transportElement);
            return elements.Clone();
        }
    }
}
