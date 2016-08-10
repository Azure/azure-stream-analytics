Scenario:
A company wants to enhance security at their campus after they found some breach where someone cloned an employee's access badge to get inside the building. They would like to monitor Badge Scan events as employees enter the building and alert on any suspicious activity like if the same badge was used more than once within 5 minute. 

Pre-Req:
Visual Studio
Azure Subscription

Running the generator code and setting up the Stream Analytics job is very simple.
This sample contains an event generator which simulates the badge scan events and sends a BadgeScan event every second to Azure Event Hub. To run the sample you will need to first create an EventHub and configure the App.config with its connection string.
You can then create a Stream Analytics Job. Configure the input to point to the EventHub your have created. In the Query Window you can copy and paste the Query below:

Note: To make the scenario more interesting in real-time, we are using a 5 second window instead of 5 minute.

SELECT System.Timestamp AS BreachTime, Alias, Count(*) AS Count
FROM BadgeEvent TIMESTAMP BY TimeStamp
GROUP BY Alias, SlidingWindow(second,5)
HAVING Count(*)>1

In Azure DB, create a SQL Database: 

CREATE DATABASE BadgeScan
GO

Then create a table with the following schema:

USE [BadgeScan]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LoginCount](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[BreachTime] [datetime2](6) NULL,
	[Alias] [nvarchar](128) NULL,
	[Count] [int] NULL,
 CONSTRAINT [PK_LoginCount] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO