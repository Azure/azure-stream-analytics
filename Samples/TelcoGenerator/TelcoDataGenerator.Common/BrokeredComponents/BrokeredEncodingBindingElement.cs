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

    class BrokeredEncodingBindingElement : MessageEncodingBindingElement
    {
        Uri listenAddress;

        public BrokeredEncodingBindingElement()
        {
        }

        // Copy ctor
        public BrokeredEncodingBindingElement(BrokeredEncodingBindingElement other)
            : base(other)
        {
        }

        public override MessageVersion MessageVersion
        {
            get
            {
                return MessageVersion.None;
            }

            set
            {
                // Only settable to MessageVersion.None
                if (value != MessageVersion.None)
                {
                    throw new ArgumentException("BrokeredMessageBindingElement_MessageVersionNoneRequired");
                }
            }
        }

        public override BindingElement Clone()
        {
            return new BrokeredEncodingBindingElement(this);
        }

        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new BrokeredEncoderFactory(this.listenAddress);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            this.listenAddress = new Uri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress);
            context.BindingParameters.Add(this);
            return base.BuildChannelListener<TChannel>(context);
        }
    }
}
