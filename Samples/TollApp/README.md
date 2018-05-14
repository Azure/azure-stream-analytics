# Azure Stream Analytics Toll App Sample

## Introduction
A toll station is a common phenomenon. You encounter them on many expressways, bridges, and tunnels across the world. Each toll station has multiple toll booths. At manual booths, you stop to pay the toll to an attendant. At automated booths, a sensor on top of each booth scans an RFID card that's affixed to the windshield of your vehicle as you pass the toll booth. It is easy to visualize the passage of vehicles through these toll stations as an event stream over which interesting operations can be performed.

Toll App is a sample streaming application that demonstrates how to use Azure Stream Analytics to get real-time insights from your data. You can easily combine streams of data, such as click-streams, logs, and device-generated events, with historical records or reference data to derive business insights. As a fully managed, real-time stream computation service that's hosted in Microsoft Azure, Azure Stream Analytics provides built-in resiliency, low latency, and scalability to get you up and running in minutes.

For more information about TollApp sample, please see <a href="https://docs.microsoft.com/en-us/azure/stream-analytics/stream-analytics-build-an-iot-solution-using-stream-analytics"> "Build an IoT solution by using Stream Analytics."</a>

For more information about the Stream Analytics service, see http://azure.microsoft.com/en-us/services/stream-analytics/

## Run this sample Toll App on Azure

### Prerequisites
To complete this solution, you need a Microsoft Azure subscription. If you do not have an Azure account, you can [request a free trial version](http://azure.microsoft.com/pricing/free-trial/).

### Deploy the TollApp template in the Azure portal
1. To deploy the TollApp environment to Azure, use this link to [Deploy TollApp Azure Template](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure%2Fazure-stream-analytics%2Fmaster%2FSamples%2FTollApp%2FVSProjects%2FTollAppDeployment%2Fazuredeploy.json).

   <a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure%2Fazure-stream-analytics%2Fmaster%2FSamples%2FTollApp%2FVSProjects%2FTollAppDeployment%2Fazuredeploy.json" target="_blank"><img src="http://azuredeploy.net/deploybutton.png"/></a>

2. Sign in to the Azure portal if prompted.

3. Choose the subscription in which the various resources are billed.

4. Specify a new resource group, with a unique name, for example `MyTollBooth`. 

5. Select an Azure location.

6. Specify an **Interval** as a number of seconds. This value is used in the sample web app, for how frequently to send data into Event Hub. 

7. **Check** to agree to the terms and conditions.

8. Select **Pin to dashboard** so that you can easily locate the resources later on.

9. Select **Purchase** to deploy the sample template.

10. After a few moments, a notification appears to confirm the **Deployment succeeded**.

### Review the Azure Stream Analytics TollApp resources
1. Log in to the Azure portal

2. Locate the Resource Group that you named in the previous section.

3. Verify that the following resources are listed in the resource group:
   - One Cosmos DB Account
   - One Azure Stream Analytics Job
   - One Azure Storage Account
   - One Azure Event Hub
   - Two Web Apps
   
## License
Microsoft Azure Stream Analytics sample application and tutorials are licensed under the MIT license. See the [LICENSE](https://github.com/Azure/azure-stream-analytics/blob/master/LICENSE.txt) file for more details.

## More information
For more information regarding this TollApp sample, see the [documentation](https://docs.microsoft.com/azure/stream-analytics/stream-analytics-build-an-iot-solution-using-stream-analytics).
