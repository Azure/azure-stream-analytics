// Serialize the events when using a compressed stream (GZipStream)
private class CompressedEventsJsonSerializer 
    { 
        public byte[] GetSerializedEvents<T>(IEnumerable<object> events) 
        { 
            if (events == null) 
            { 
                throw new ArgumentNullException(nameof(events)); 
            } 
 
            JsonSerializer serializer = new JsonSerializer(); 
 
            using (var memoryStream = new MemoryStream()) 
            using (FileStream file = new FileStream(@"C:\states1.gz", FileMode.OpenOrCreate)) 
            using (GZipStream stream = new GZipStream(file, CompressionLevel.Optimal)) 
            using (StreamWriter sw = new StreamWriter(stream)) 
            using (JsonWriter writer = new JsonTextWriter(sw)) // or using (StringWriter writer = new StringWriter(sw)) 
            { 
                foreach (var current in events) 
                { 
                    serializer.Serialize(writer, current); 
                } 
                return memoryStream.ToArray(); 
            } 
        } 
    } 

    // Send the events to EventHub
    private static void Main(string[] args) 
    { 
        var eventHubClient = 
            EventHubClient.CreateFromConnectionString( 
                "<ReplaceWithServiceBusConnectionString>", 
                "<ReplaceWithEventHubName>"); 
        var compressedEventsJsonSerializer = new CompressedEventsJsonSerializer(); 
        while (true) 
        { 
            var eventsPayload = 
                compressedEventsJsonSerializer.GetSerializedEvents( 
                    Enumerable.Range(0, 5).Select(i => new SampleEvent() { Id = i })); 
            eventHubClient.Send(new EventData(eventsPayload)); 
            Thread.Sleep(TimeSpan.FromSeconds(10)); 
        } 
    } 
