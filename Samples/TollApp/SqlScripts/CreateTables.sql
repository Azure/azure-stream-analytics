-- TUMBLING WINDOW EXAMPLE
IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = 'TollDataTumblingCount'))
BEGIN
CREATE TABLE [dbo].[TollDataTumblingCount] (
    [TollId] INT  NULL,
    [WindowEnd]   DATETIME2  NULL,
    [Count]         INT     NULL
);
END

IF (NOT EXISTS (SELECT * FROM SYS.INDEXES 
                 WHERE NAME = 'IX_TollDataTumblingCount_StartTime'))
BEGIN
CREATE CLUSTERED INDEX [IX_TollDataTumblingCount_StartTime]
    ON [dbo].[TollDataTumblingCount]([WindowEnd] ASC);
END


-- JOIN EXAMPLE

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = 'TollDataJoin'))
BEGIN
CREATE TABLE [dbo].[TollDataJoin] (
    [TollId] INT  NULL,
    [EntryTime] DATETIME2  NULL,
    [ExitTime]   DATETIME2  NULL,
    [LicensePlate]   NVARCHAR(max)  NULL,
    [DurationInMinutes]       INT     NULL
);
END

IF (NOT EXISTS (SELECT * FROM SYS.INDEXES 
                 WHERE NAME = 'IX_TollDataJoin_EntryTime'))
BEGIN
CREATE CLUSTERED INDEX [IX_TollDataJoin_EntryTime]
    ON [dbo].[TollDataJoin]([EntryTime] ASC);
END

-- REFERENCE DATA JOIN EXAMPLE

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = 'TollDataRefJoin'))
BEGIN
CREATE TABLE [dbo].[TollDataRefJoin] (
    [EntryTime] DATETIME2  NULL,
    [LicensePlate]   NVARCHAR(max)  NULL,
    [TollId] INT  NULL,
    [RegistrationId]   BIGINT  NULL
);
END

IF (NOT EXISTS (SELECT * FROM SYS.INDEXES 
                 WHERE NAME = 'IX_TollDataRefJoin_EntryTime'))
BEGIN
CREATE CLUSTERED INDEX [IX_TollDataRefJoin_EntryTime]
    ON [dbo].[TollDataRefJoin]([EntryTime] ASC);
END

-- TUMBLING WINDOW PARTITIONED EXAMPLE
IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = 'TollDataTumblingCountPartitioned'))
BEGIN
CREATE TABLE [dbo].[TollDataTumblingCountPartitioned] (
    [TollId] INT  NULL,
    [WindowEnd]   DATETIME2  NULL,
    [Count]         INT     NULL
);
END

IF (NOT EXISTS (SELECT * FROM SYS.INDEXES 
                 WHERE NAME = 'IX_TollDataTumblingCountPartitioned_StartTime'))
BEGIN
CREATE CLUSTERED INDEX [IX_TollDataTumblingCountPartitioned_StartTime]
    ON [dbo].[TollDataTumblingCountPartitioned]([WindowEnd] ASC);
END
