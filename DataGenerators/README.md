# Microsoft Azure Stream Analytics #

This folder contains event generators for Azure Stream Analytics.

Use [TelcoGeneratorCore](TelcoGeneratorCore) for the phone-call tutorials. It targets .NET 8 and uses `Azure.Messaging.EventHubs`.

`SecurityBadgeScan`, `SensorEventGenerator`, and the original `TelcoGenerator` use `WindowsAzure.ServiceBus`, which retired for Event Hubs on September 30, 2026. They are historical examples, not current starting points. See the [retirement and migration guidance](../README.md#event-generator-library-retirement).
 
For tutorials that reference these samples, see [http://azure.microsoft.com/en-us/documentation/services/stream-analytics/](http://azure.microsoft.com/en-us/documentation/services/stream-analytics/)

For more information about the Stream Analytics service, please see [http://azure.microsoft.com/en-us/services/stream-analytics/](http://azure.microsoft.com/en-us/services/stream-analytics/)
