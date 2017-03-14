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
    using System.IO;
    using System.ServiceModel.Channels;

    // Creates an empty WCF message to be stamped by NetMessagingTransportBindingElement
    // The NetMessagingTransportBindingElement stamps BrokeredMessageProperty onto 
    // the WCF message.
    class BrokeredEncoder : MessageEncoder
    {
        BrokeredEncoderFactory factory;
        Uri listenUri;

        public BrokeredEncoder(BrokeredEncoderFactory factory, Uri listenUri)
        {
            this.factory = factory;
            this.listenUri = listenUri;
        }

        public override MessageVersion MessageVersion
        {
            get { return MessageVersion.None; }
        }

        public override string MediaType
        {
            get { throw new NotSupportedException(); }
        }

        public override string ContentType
        {
            get { throw new NotSupportedException(); }
        }

        // Not used since writing to topics is supported via MessageSender
        // BrokeredMessageEncoder is only used to read
        public override ArraySegment<byte> WriteMessage(
            Message message, 
            int maxMessageSize, 
            BufferManager bufferManager, 
            int messageOffset)
        {
            throw new NotSupportedException();
        }

        // Not used since writing to topics is supported via MessageSender
        // BrokeredMessageEncoder is only used to read
        public override void WriteMessage(Message message, Stream stream)
        {
            throw new NotSupportedException();
        }

        // Uses CreateMessage() to create an empty message with the appropriate action
        public override Message ReadMessage(
            ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            return this.CreateMessage();
        }

        // Uses CreateMessage() to create an empty message with the appropriate action
        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            return this.CreateMessage();
        }

        // Creates an empty message and stamps it with the listenUri to let 
        // the dispatcher know what to call.
        private Message CreateMessage()
        {
            Message message = Message.CreateMessage(MessageVersion.None, String.Empty);
            message.Headers.To = this.listenUri;
            return message;
        }
    } 
}
