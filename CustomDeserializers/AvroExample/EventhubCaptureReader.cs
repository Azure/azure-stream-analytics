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


using System.Collections.Generic;
using System.IO;

using Avro.Generic;
using Avro.IO;
using Avro.File;
using Microsoft.Azure.StreamAnalytics;
using Microsoft.Azure.StreamAnalytics.Serialization;

namespace ExampleCustomCode.AvroSerialization
{
    // Reads eventhub capture format. https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-capture-overview
    public abstract class EventhubCaptureReader<T> : StreamDeserializer<T>
    {
        protected StreamingDiagnostics diagnostics;

        public override void Initialize(StreamingContext streamingContext)
        {
            this.diagnostics = streamingContext.Diagnostics;
        }

        public override IEnumerable<T> Deserialize(Stream stream)
        {
            var reader = DataFileReader<GenericRecord>.OpenReader(stream);
            foreach(GenericRecord genericRecord in reader.NextEntries)
            {
                var eventData = new EventDataFromCapture()
                {
                    EnqueuedTimeUtc = (string)genericRecord["EnqueuedTimeUtc"],
                    Offset = (string)genericRecord["Offset"],
                    SequenceNumber = (long)genericRecord["SequenceNumber"],
                    Body = (byte[])genericRecord["Body"],
                };

                // deserialize records from eventdata body.  
                foreach (T record in this.DeserializeEventData(eventData))
                {
                    yield return record;
                }
            }
            
            reader.Dispose();
        }

        protected abstract IEnumerable<T> DeserializeEventData(EventDataFromCapture eventData);
    }

    public class EventDataFromCapture
    {
        public long SequenceNumber { get; set; }
        public string Offset { get; set; }
        public string EnqueuedTimeUtc { get; set; }

        public byte[] Body { get; set; }
    }
}