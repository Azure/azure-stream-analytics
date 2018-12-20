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
            IFileReader<GenericRecord> reader = null;
            try
            {
                reader = DataFileReader<GenericRecord>.OpenReader(stream);
            }
            catch(Exception e)
            {
                this.diagnostics.WriteError(
                    briefMessage: "Unable to open stream as avro. Please check if the stream is from eventhub capture. https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-capture-overview ",
                    detailedMessage: e.Message);
                throw;
            }
            
            foreach(GenericRecord genericRecord in reader.NextEntries)
            {
                EventDataFromCapture eventData = this.ConvertToEventDataFromCapture(genericRecord);

                // deserialize records from eventdata body.  
                foreach (T record in this.DeserializeEventData(eventData))
                {
                    yield return record;
                }
            }
            
            reader.Dispose();
        }

        protected abstract IEnumerable<T> DeserializeEventData(EventDataFromCapture eventData);

        private EventDataFromCapture ConvertToEventDataFromCapture(GenericRecord genericRecord)
        {
            try
            {
                var eventData = new EventDataFromCapture()
                {
                    EnqueuedTimeUtc = (string)genericRecord["EnqueuedTimeUtc"],
                    Offset = (string)genericRecord["Offset"],
                    SequenceNumber = (long)genericRecord["SequenceNumber"],
                    Body = (byte[])genericRecord["Body"],
                };

                return eventData;
            }
            catch(Exception e)
            {
                this.diagnostics.WriteError(
                    briefMessage: $"Unable to get fields required to create captured event data. Error: {e.Message}",
                    detailedMessage: e.ToString());
                throw;
            }
        }
    }

    public class EventDataFromCapture
    {
        public long SequenceNumber { get; set; }
        public string Offset { get; set; }
        public string EnqueuedTimeUtc { get; set; }

        public byte[] Body { get; set; }
    }
}