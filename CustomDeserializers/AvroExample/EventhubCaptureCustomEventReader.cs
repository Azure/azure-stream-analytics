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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

using ExampleCustomCode.Serialization;

using Microsoft.Azure.StreamAnalytics;

namespace ExampleCustomCode.AvroSerialization
{
    // Reads eventhub capture in avro format and deserializes EventData.Body as a simple record.
    public sealed class EventhubCaptureCustomEventReader : EventhubCaptureReader<EventHubRecord>
    {
        private readonly ExampleDeserializer contentDeserializer = new ExampleDeserializer();

        public override void Initialize(StreamingContext streamingContext)
        {
            this.contentDeserializer.Initialize(streamingContext);
            base.Initialize(streamingContext);
        }

        protected override IEnumerable<EventHubRecord> DeserializeEventData(EventDataFromCapture eventData)
        {
            // assumes EventData.Body is a gzipped line separated records.
            using (var stream = new MemoryStream(eventData.Body))
            {
                try
                {
                    // deserialize body from a single eventdata completely. Skip eventData message body with invalid format.
                    using (var unzippedStream = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        return this.contentDeserializer
                        .Deserialize(unzippedStream)
                        .Select(
                            payload => new EventHubRecord()
                            {
                                Offset = eventData.Offset,
                                Payload = payload
                            })
                        .ToArray();
                    }
                }
                catch (Exception e)
                {
                    var shortErrorMessage = $"Error deserializing eventhub message with offset: {eventData.Offset} SequenceNumber:{eventData.SequenceNumber}";
                    this.diagnostics.WriteError(
                        shortErrorMessage,
                        $"{shortErrorMessage} Exception: {e}");
                    return Enumerable.Empty<EventHubRecord>();
                }
            }
        }
    }

    public class EventHubRecord
    {
        public string Offset { get; set; }
        public CustomEvent Payload { get; set; }
    }
}
