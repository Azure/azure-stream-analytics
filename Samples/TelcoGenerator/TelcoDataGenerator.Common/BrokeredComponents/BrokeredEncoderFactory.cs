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

    // Factory wrapper around BrokeredMessageEncoder
    class BrokeredEncoderFactory : MessageEncoderFactory
    {
        BrokeredEncoder encoder;

        public BrokeredEncoderFactory(Uri listenUri)
        {
            this.encoder = new BrokeredEncoder(this, listenUri);
        }

        public override MessageEncoder Encoder
        {
            get 
            {
                return this.encoder; 
            }
        }

        public override MessageVersion MessageVersion
        {
            get 
            { 
                return this.encoder.MessageVersion; 
            }
        }
    }
}
