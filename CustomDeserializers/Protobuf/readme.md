This is a sample implementation of protobuf deserializer. The SampleDefinition.proto was used to generate the .cs file with the definition (MessageBodyProto.cs). The SimulatedTemperatureEvents.protobuf is a sample input that can be used to test this custom deserializer implementation. This sample input has over 1000 records. An example of record in this file (in JSON format) would be

```
{
    "Ambient": {
        "Humidity": 25,
        "Temperature": 21.408860853365091
    },
    "Machine": {
        "Pressure": 1.0009042287864474,
        "Temperature": 22.002588784439762
    },
    "TimeCreated": "2018-09-17T17:52:36.4353656Z"
}
```

To learn how to try out this sample, please follow the [Visual Studio Walkthrough](https://aka.ms/asadeserializer).
