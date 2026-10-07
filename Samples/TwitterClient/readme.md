# Historical Twitter producer

This folder contains the original Twitter producer and compiled files. It uses `WindowsAzure.ServiceBus`, which retired for Event Hubs on September 30, 2026. Do not use this binary as a current event generator. Before reusing its source, migrate the producer using the [Event Hubs migration guide](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/eventhub/Azure.Messaging.EventHubs/MigrationGuide_WindowsAzureServiceBus.md) and verify the external data-source API requirements.
