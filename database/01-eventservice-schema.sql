/* ============================================================================
   Event Platform - EventService database (SQL Server)
   ----------------------------------------------------------------------------
   Run this script in SQL Server Management Studio (SSMS) / Azure Data Studio.
   It creates the EventPlatform database, the domain tables (Events, Zones),
   the MassTransit transactional-outbox tables and a small seed dataset.

   The script is idempotent: it can be executed multiple times safely.
   ============================================================================ */

IF DB_ID(N'EventPlatform') IS NULL
BEGIN
    CREATE DATABASE [EventPlatform];
END;
GO

USE [EventPlatform];
GO

/* ----------------------------- EF migrations history ---------------------- */
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

/* --------------------------------- Domain tables -------------------------- */
IF OBJECT_ID(N'[Events]') IS NULL
BEGIN
    CREATE TABLE [Events] (
        [EventId]   uniqueidentifier NOT NULL,
        [Name]      nvarchar(200)    NOT NULL,
        [EventDate] datetimeoffset   NOT NULL,
        [Place]     nvarchar(200)    NOT NULL,
        [Status]    nvarchar(20)     NOT NULL,
        [RowVersion] rowversion      NULL,
        CONSTRAINT [PK_Events] PRIMARY KEY ([EventId])
    );
    CREATE INDEX [IX_Events_EventDate] ON [Events] ([EventDate]);
END;
GO

IF OBJECT_ID(N'[Zones]') IS NULL
BEGIN
    CREATE TABLE [Zones] (
        [ZoneId]   uniqueidentifier NOT NULL,
        [EventId]  uniqueidentifier NOT NULL,
        [Name]     nvarchar(100)    NOT NULL,
        [Price]    decimal(10,2)    NOT NULL,
        [Capacity] int              NOT NULL,
        CONSTRAINT [PK_Zones] PRIMARY KEY ([ZoneId]),
        CONSTRAINT [FK_Zones_Events_EventId] FOREIGN KEY ([EventId])
            REFERENCES [Events] ([EventId]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_Zones_EventId] ON [Zones] ([EventId]);
END;
GO

/* ----------------------- MassTransit outbox / inbox ----------------------- */
IF OBJECT_ID(N'[InboxState]') IS NULL
BEGIN
    CREATE TABLE [InboxState] (
        [Id] bigint NOT NULL IDENTITY,
        [MessageId] uniqueidentifier NOT NULL,
        [ConsumerId] uniqueidentifier NOT NULL,
        [LockId] uniqueidentifier NOT NULL,
        [RowVersion] rowversion NULL,
        [Received] datetime2 NOT NULL,
        [ReceiveCount] int NOT NULL,
        [ExpirationTime] datetime2 NULL,
        [Consumed] datetime2 NULL,
        [Delivered] datetime2 NULL,
        [LastSequenceNumber] bigint NULL,
        CONSTRAINT [PK_InboxState] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_InboxState_MessageId_ConsumerId] UNIQUE ([MessageId], [ConsumerId])
    );
    CREATE INDEX [IX_InboxState_Delivered] ON [InboxState] ([Delivered]);
END;
GO

IF OBJECT_ID(N'[OutboxState]') IS NULL
BEGIN
    CREATE TABLE [OutboxState] (
        [OutboxId] uniqueidentifier NOT NULL,
        [LockId] uniqueidentifier NOT NULL,
        [RowVersion] rowversion NULL,
        [Created] datetime2 NOT NULL,
        [Delivered] datetime2 NULL,
        [LastSequenceNumber] bigint NULL,
        [BusName] nvarchar(256) NULL,
        CONSTRAINT [PK_OutboxState] PRIMARY KEY ([OutboxId])
    );
    CREATE INDEX [IX_OutboxState_BusName_Created] ON [OutboxState] ([BusName], [Created]);
    CREATE INDEX [IX_OutboxState_Created] ON [OutboxState] ([Created]);
END;
GO

IF OBJECT_ID(N'[OutboxMessage]') IS NULL
BEGIN
    CREATE TABLE [OutboxMessage] (
        [SequenceNumber] bigint NOT NULL IDENTITY,
        [EnqueueTime] datetime2 NULL,
        [SentTime] datetime2 NOT NULL,
        [Headers] nvarchar(max) NULL,
        [Properties] nvarchar(max) NULL,
        [InboxMessageId] uniqueidentifier NULL,
        [InboxConsumerId] uniqueidentifier NULL,
        [OutboxId] uniqueidentifier NULL,
        [MessageId] uniqueidentifier NOT NULL,
        [ContentType] nvarchar(256) NOT NULL,
        [MessageType] nvarchar(max) NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [ConversationId] uniqueidentifier NULL,
        [CorrelationId] uniqueidentifier NULL,
        [InitiatorId] uniqueidentifier NULL,
        [RequestId] uniqueidentifier NULL,
        [SourceAddress] nvarchar(256) NULL,
        [DestinationAddress] nvarchar(256) NULL,
        [ResponseAddress] nvarchar(256) NULL,
        [FaultAddress] nvarchar(256) NULL,
        [ExpirationTime] datetime2 NULL,
        CONSTRAINT [PK_OutboxMessage] PRIMARY KEY ([SequenceNumber]),
        CONSTRAINT [FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId] FOREIGN KEY ([InboxMessageId], [InboxConsumerId]) REFERENCES [InboxState] ([MessageId], [ConsumerId]),
        CONSTRAINT [FK_OutboxMessage_OutboxState_OutboxId] FOREIGN KEY ([OutboxId]) REFERENCES [OutboxState] ([OutboxId])
    );
    CREATE INDEX [IX_OutboxMessage_EnqueueTime] ON [OutboxMessage] ([EnqueueTime]);
    CREATE INDEX [IX_OutboxMessage_ExpirationTime] ON [OutboxMessage] ([ExpirationTime]);
    EXEC(N'CREATE UNIQUE INDEX [IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber] ON [OutboxMessage] ([InboxMessageId], [InboxConsumerId], [SequenceNumber]) WHERE [InboxMessageId] IS NOT NULL AND [InboxConsumerId] IS NOT NULL');
    EXEC(N'CREATE UNIQUE INDEX [IX_OutboxMessage_OutboxId_SequenceNumber] ON [OutboxMessage] ([OutboxId], [SequenceNumber]) WHERE [OutboxId] IS NOT NULL');
END;
GO

/* Mark the EF migration as applied so `dotnet ef database update` is a no-op. */
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260606170343_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260606170343_InitialCreate', N'9.0.8');
END;
GO

/* --------------------------------- Seed data ----------------------------- */
IF NOT EXISTS (SELECT 1 FROM [Events])
BEGIN
    DECLARE @e1 uniqueidentifier = '11111111-1111-1111-1111-111111111111';
    DECLARE @e2 uniqueidentifier = '22222222-2222-2222-2222-222222222222';

    INSERT INTO [Events] ([EventId], [Name], [EventDate], [Place], [Status]) VALUES
        (@e1, N'Atlantic City Music Festival', '2026-09-15T20:00:00+00:00', N'Atlantic City Arena', N'Published'),
        (@e2, N'Tech Summit 2026',             '2026-11-02T09:00:00+00:00', N'Lima Convention Center', N'Published');

    INSERT INTO [Zones] ([ZoneId], [EventId], [Name], [Price], [Capacity]) VALUES
        (NEWID(), @e1, N'VIP',     250.00, 100),
        (NEWID(), @e1, N'General', 80.00,  5000),
        (NEWID(), @e2, N'Standard',120.00, 1200),
        (NEWID(), @e2, N'Premium', 300.00, 200);
END;
GO

SELECT * FROM [Events];
SELECT * FROM [Zones];
GO
