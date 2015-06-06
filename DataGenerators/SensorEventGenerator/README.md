# SensorEventGenerator #

This solution simulates SensorTag events and pushes them to an Event Hub instance. For more information about scenarios and usage, see [here](http://gallery.azureml.net/Tutorial/6f95aeaca0fc43b3aec37ad3a0526a21)



You may use this event simulator when you don't have a TI SensorTag and want to experience the SensorTag scenario explained in the above link using ASA.
```
You will have to create an EventHub(instructions in the link above) and configure the connection string properties in the App.Config file included in the project
<add key="EventHubConnectionString" value="***YOUR EVENT HUB CONNECTION STRING***" />
<add key="EventHubName" value="***YOUR EVENT HUB NAME***" />
```
