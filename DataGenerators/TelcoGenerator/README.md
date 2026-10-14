# Retired TelcoGenerator

This original phone-call generator uses `WindowsAzure.ServiceBus`, which retired for Event Hubs on September 30, 2026. Use [TelcoGeneratorCore](../TelcoGeneratorCore), which targets .NET 8 and uses `Azure.Messaging.EventHubs`. Follow the [current fraud-detection tutorial](https://learn.microsoft.com/azure/stream-analytics/stream-analytics-real-time-fraud-detection).
