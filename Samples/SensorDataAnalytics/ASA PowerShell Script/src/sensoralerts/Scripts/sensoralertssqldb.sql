--Create Tables
USE [SensorAlerts]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TemperatureAlerts](
    [Id] [bigint] IDENTITY(1,1) NOT NULL,
    [OutputTime] [datetime2](6) NULL,
    [SensorName] [nvarchar](128) NULL,
    [AvgTemperature] [float] NULL,
 CONSTRAINT [PK_TemperatureAlerts] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO