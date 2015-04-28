# Azure Stream Analytics with Power BI 
#### *Building a Demo with TI Bluetooth sensor, Azure Stream Analytics & Power BI* 

### Prerequisites for this demo

* TI Sensor that connects to windows machine via Bluetooth. [Here is one link to buy this sensor](http://www.newark.com/texas-instruments/cc2541dk-sensor/dev-board-cc2541-2-4ghz-bluetooth/dp/55W6125?mckv=stpn1QPcu|pcrid|57087234021|plid|&CMP=KNC-GUSA-GEN-SHOPPING-TEXAS_INSTRUMENTS) (I am sure you can find it at other places too) 

* Windows Azure Account using Org Id (Power BI works with Org ID only. Org ID is your work or business email address e.g. xyz@mycompany.com. Personal emails like xyz@hotmail.com are not org ids. [You can learn more about org id here](https://www.arin.net/resources/request/org.html) )

* An eventhub with a shared access key with "manage" permission. If you are new to EventHub, you can find [information on creating eventhub over here](http://azure.microsoft.com/en-us/documentation/articles/service-bus-event-hubs-csharp-ephcs-getstarted/). Don't worry about how to send events to eventhub for this demo because the windows 8  desktop app created for this demo will help you do that. 

* Windows 8 laptop that can connect to TI sensor via bluetooth. 
  * Note- Windows 8 is NOT a requirement for Azure services. This is a requirement only for this particular Windows 8 desktop app used for this demo to read data from the TI sensor. This desktop app is just a quick and dirty app to demonstrate how can you send data from TI sensor to Azure Eventhub. You can write your own app on any platform to do the same. Azure eventhub developer guide can help you in that.

### Sending data from TI Bluetooth sensor to Eventhub
* Copy all the files from the [DeploymentFiles folder of this github repository](https://github.com/sudheshk/TISensorToEventHub_WindowsForm/tree/master/DeploymentFiles) to your local machine. If you want to take a look at source code and modify for your scenario, source code is available at the [SourceCode folder of the same github repository](https://github.com/sudheshk/TISensorToEventHub_WindowsForm/tree/master/SourceCode)
* Change following configs in SensorTagToEventHub.exe.config config file for your Event Hub, Service Bus and access policy name and access key. As noted in the prerequisite section, this key should have "manage" permission on your eventhub:
```.net
  <appSettings>
    <add key="SBNameSpace" value="your service bus" />    [this is the namespace of your servicebus]
    <add key="EHName" value="your event hub" />  [This is the ame of your eventhub]
    <add key="AccessPolicyName" value="your policy name" />  [This is the name of shared access policy name on your servicebus or eventhub with manage permission]
    <add key="AccessPolicyKey" value="your key" />  [This is the  shared access policy key on your servicebus or eventhub with manage permission]
    <add key="SensorName" value="your sensor" />  [Any display name you want to give to your sensor]
  </appSettings>
```

* Connect your TI Sensor to your windows 8 machine with bluetooth connectivity. Go to Settings -> Bluetooth -> You'll see SensorTag as a device to pair. Click on Pair. When asked for password, use 0. [0 is the default password for these sensor tags]. Once you see this sensor connected as a bluetooth device, you are all set to send data to your eventhub.

* Run SensorTagToEventHub.exe. It is an exe in the DeploymentFiles folder that you copied from the github respository. 

* Click on "Send Data to Eventhub" button. If your TI sensor is connected properly to your windows 8 machine via bluetooth, this desktop app will start sending data to your eventhub. Note- To be on the safer side, please remove and re-pair the TI sensor to your windows 8 device everytime you start a new demo (esp. after your computer is waking up from sleep)

### Create ASA Job and choose Power BI as output
* On Azure Portal, choose Azure Stream Analytics service and quick create a new ASA job (choose name, region, monitoring storage account as necessary). 

* For input, choose the eventhub you are sending data to as the input. Give it alias "Input"

* Go to Output tab. If you are part of Power BI preview program, you'll see Power BI as an output option as below:

  ![Power BI Output for ASA](/images/PowerBIOutput.PNG)

* On selecting Power BI output, it'll ask you to authorize or signup for powerbi. At this time, you have to use the same org id that you used to create the ASA job. 

* Choose your output name , I called it "outputpbi". Put a dataset name and table name that you want. At the time of this writing, you can have only 1 table per dataset for Power BI output from ASA

* You don't need to pre-create this dataset and table in Power BI. ASA job will create them for you when the job runs and pushes the output to Power BI. Note- if there is no output from ASA Job, these datasets will not be created. 

* Another point to note is that if you already have a dataset in Power BI using the same name that you specified in your ASA job, ASA job will overwrite that dataset.

* For the query of the job, I used following query. Feel free to modify for your scenario however make sure you are using proper columns names, inout, output alias. If you are using multiple outputs, ensure that you are using "SELECT...INTO..." syntax. Anyway, my query is:
```sql
SELECT 
        max(hmdt) as hmdt ,
        max(temp) as temp ,
        time ,
        0 as minTemp ,
        150 as maxTemp ,
        75 as targetTemp ,
        100 as maxHmdt ,
        70 as targetHmdt 
FROM Input 
WHERE dspl = 'your sensor'  --Put your sensor name that you are passing through the windows 8 desktop app for sending data to eventhub or just remove the where clause
Group by TUMBLINGWINDOW(ss,1) ,time ,dspl

 ```

* Start the ASA job

* Assuming that your eventhub is still getting sensor events through your windows 8 desktop app and your query is pumping out query result to output, you will see PowerBI.com showing this dataset created.

* Now in Power BI, you can create interesting charts on top of this datasets. I created following to show average temprature and average humidity of 1 second window over time. If you pin the chart to your dashboard, you will see the dashboard updating data in realtime (live dashboard with real time updates)

   ![Power Bi Charts](/images/powerbicharts.PNG)

* To show the impact of live dashboard, try blowing into the sensor. Within a few seconds, you will the chart spiking up for humidity and temperature.  

* That is the end of the demo.   
