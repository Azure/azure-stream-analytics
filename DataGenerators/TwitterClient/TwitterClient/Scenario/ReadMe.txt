Scenario:
A news media website is interested in getting an edge over its competitors by featuring site content that is immediately relevant to its readers. They use social media analysis on topics relevant to their readers by doing real time sentiment analysis on Twitter data. Specifically, to identify what topics are trending in real time on Twitter, they need real-time analytics about the tweet volume and sentiment for key topics.

Pre-Req:
Visual Studio
Azure Subscription
Twitter Account

Running the generator code and setting up the Stream Analytics job is very simple.
This sample contains an event generator which calls the Twitter API (dev.twitter.com) to get tweet events. Application parses tweets for parameterized keywords (Azure,Microsoft,Seattle, etc.) and uses Sentiment140 (www.sentiment140.com) to add sentiment score to tweet events. To run the sample you will need to first create an EventHub and configure the App.config with its connection string.
You can then create a Stream Analytics Job. Configure the input to point to the EventHub you have created. In the Query Window you can copy and paste the Query below:


SELECT Topic,count(*) AS Count, Avg(SentimentScore) AS AvgSentiment, System.Timestamp AS Insert_Time
FROM TwitterInput TIMESTAMP BY CreatedAt
GROUP BY TumblingWindow(second,5), Topic

To see the output in a SQL table, you will need to create a SQL table with the command below and configure a SQL output for your ASA job to point to the database you have created.

In Azure DB, create a SQL Database:

CREATE DATABASE TwitterDemo
GO

Then create a table with the schema below:

USE [TweetCount]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TweetCount](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Topic] [nvarchar](128) NULL,
	[Count] [int] NULL,
	[AvgSentiment] [float] NULL,
	[Insert_Time] [datetime2](6) NULL,
 CONSTRAINT [PK_TweetCount] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO

Please note: This sample code calls the Twitter API, which is provided by Twitter, Inc. and not by Microsoft, to obtain tweets. Your use of the Twitter API is governed by your agreement(s) with Twitter, Inc.  This sample code also calls the API from Sentiment140 (www.sentiment140.com), which is not affiliated with Microsoft, to discover the sentiment of tweets. Your use of the Sentiment140 API is governed by any terms imposed on your use by the owners of the Sentiment140 API.