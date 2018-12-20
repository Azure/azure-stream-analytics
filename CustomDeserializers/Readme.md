Contains example user defined deserializers for azure stream analytics.<br/>
Interfaces for deserializers are defined in [this nuget](https://www.nuget.org/packages/Microsoft.Azure.StreamAnalytics/).

`ExampleDeserializer` : 
- Contains a simple deserializer that treats each line as a record.

`EventhubCaptureReader<T>` : 
- Abstract class with logic to read [EventHub capture](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-capture-overview) format.
- To use `EventhubCaptureReader` inherit and implement `DeserializeEventData<T>`

`EventhubCaptureCustomEventReader` :
- Example implementation of `EventhubCaptureReader`.
- Assumes eventhub message body is gzip compressed.
- Uses `ExampleDeserializer` to deserialize message body.

Run `dotnet publish` to create package with all necessary binaries.<br/>