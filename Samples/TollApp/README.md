# Azure Stream Analytics Toll App
For more information about TollApp sample, please see <a href="https://docs.microsoft.com/en-us/azure/stream-analytics/stream-analytics-build-an-iot-solution-using-stream-analytics"> "Build an IoT solution by using Stream Analytics."</a>
For more information about the Stream Analytics service, see http://azure.microsoft.com/en-us/services/stream-analytics/

**Introduction**

In this tutorial, you will learn how to use Azure Stream Analytics to get real-time insights from your data. Developers can easily combine streams of data, such as click-streams, logs, and device-generated events, with historical records or reference data to derive business insights. As a fully managed, real-time stream computation service that's hosted in Microsoft Azure, Azure Stream Analytics provides built-in resiliency, low latency, and scalability to get you up and running in minutes.

After completing this tutorial, you will be able to:

    *Familiarize yourself with the Azure Stream Analytics portal.
    *Configure and deploy a streaming job.
    *Articulate real-world problems and solve them by using the Stream Analytics query language.
    *Develop streaming solutions for your customers by using Stream Analytics with confidence.
    *Use the monitoring and logging experience to troubleshoot issues.

## Resources
### Prerequisites
* A Microsoft account http://account.live.com
* A Microsoft Azure account http://azure.microsoft.com/en-us/pricing/free-trial/

### Documentation
### Scenario introduction: “Hello, Toll!”**

A toll station is a common phenomenon. You encounter them on many expressways, bridges, and tunnels across the world. Each toll station has multiple toll booths. At manual booths, you stop to pay the toll to an attendant. At automated booths, a sensor on top of each booth scans an RFID card that's affixed to the windshield of your vehicle as you pass the toll booth. It is easy to visualize the passage of vehicles through these toll stations as an event stream over which interesting operations can be performed.

To deploy the app to Azure, click the link below. Deploy the app in a new resource group, select an Azure location, and provide a short *user* value that will be appended to several resource names to make them globally unique. Your initials and a number is a good pattern to use.

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fgithub.com%2Fmeyergm%2Fazure-stream-analytics%2Fblob%2Fmaster%2FSamples%2FTollApp%2FTollAppDeployment%2Fazuredeploy.json" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a>

## License
Microsoft Azure Stream Analytics sample application and tutorials are licensed under the MIT license. See the [LICENSE](https://github.com/Azure/azure-stream-analytics/blob/master/LICENSE.txt) file for more details.