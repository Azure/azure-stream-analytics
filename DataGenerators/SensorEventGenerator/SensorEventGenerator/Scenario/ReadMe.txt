Scenario:
Scenario: Real-time alerting and RCA over sensor data
Customers like Contoso manufacturing plant has sensors emitting data. Production floor manager wants to be alerted whenever a single or set of sensor value exceeds a given threshold in a temporal window. So, ASA analyzes sensor data and sends alert to Power BI dashboard and user also gets alert on the  smart phone. 
However Production Manager also wants to later analyze, and perform root cause analysis on which specific sensors a lot of these alerts in the last week, last month or so. To meet this requirement, ASA is also sending output of these sensor output to DocDB. Caterpillar has built nice dashboarding and analytic applications on top of this sensor data in DocDB to provide customers views/access to aggregated event data.
 
Pattern – Sensors -> EH -> ASA ->  DocDB -> RCA apps
           			             ->Real time alerting and dashboarding          

Pre-Req:
Visual Studio
Azure Subscription



WITH First AS (
SELECT SensorName, Avg(temperature), System.Timestamp AS OutputTime 
FROM EntryStream TIMESTAMP BY Timestamp
Group By TumblingWindow(second,5),SensorName
)
SELECT * INTO PowerBI FROM First;
SELECT * INTO EventHub FROM First

