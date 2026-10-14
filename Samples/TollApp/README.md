# Historical tollbooth solution

This solution's event producer uses `WindowsAzure.ServiceBus`, which retired for Event Hubs on September 30, 2026. The source is retained for reference; the old one-click deployment is no longer a recommended way to start with Stream Analytics.

For a current end-to-end walkthrough, use the [fraud-detection tutorial](https://learn.microsoft.com/azure/stream-analytics/stream-analytics-real-time-fraud-detection) with [TelcoGeneratorCore](../../DataGenerators/TelcoGeneratorCore).

If you need to reuse the tollbooth scenario, migrate its producer using the [Event Hubs migration guide](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/eventhub/Azure.Messaging.EventHubs/MigrationGuide_WindowsAzureServiceBus.md) before deploying the application.

## License

This sample is licensed under the MIT license. See [LICENSE](../../LICENSE.txt).
