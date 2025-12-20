-- Migration: Add Chat Feature
-- Description: Adds ChatRooms, ChatMessages, and ChatRoomMemberships tables for real-time chat functionality

-- Create ChatRooms table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatRooms')
BEGIN
    CREATE TABLE [dbo].[ChatRooms] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [RoomType] NVARCHAR(50) NOT NULL,
        [RequiredRole] NVARCHAR(50) NULL,
        [IsSystemRoom] BIT NOT NULL DEFAULT 0,
        [OwnerId] NVARCHAR(450) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL
    );
END
GO

-- Create ChatMessages table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatMessages')
BEGIN
    CREATE TABLE [dbo].[ChatMessages] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [ChatRoomId] UNIQUEIDENTIFIER NOT NULL,
        [SenderId] NVARCHAR(450) NOT NULL,
        [SenderName] NVARCHAR(100) NOT NULL,
        [Content] NVARCHAR(MAX) NOT NULL,
        [Timestamp] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [IsRead] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT [FK_ChatMessages_ChatRooms] FOREIGN KEY ([ChatRoomId]) 
            REFERENCES [dbo].[ChatRooms]([Id]) ON DELETE CASCADE
    );

    -- Create index for faster queries
    CREATE INDEX [IX_ChatMessages_ChatRoomId_Timestamp] 
        ON [dbo].[ChatMessages]([ChatRoomId], [Timestamp]);
END
GO

-- Create ChatRoomMemberships table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatRoomMemberships')
BEGIN
    CREATE TABLE [dbo].[ChatRoomMemberships] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [ChatRoomId] UNIQUEIDENTIFIER NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [IsRoomAdmin] BIT NOT NULL DEFAULT 0,
        [IsMuted] BIT NOT NULL DEFAULT 0,
        [JoinedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [LastReadAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        CONSTRAINT [FK_ChatRoomMemberships_ChatRooms] FOREIGN KEY ([ChatRoomId]) 
            REFERENCES [dbo].[ChatRooms]([Id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_ChatRoomMemberships_UserId_ChatRoomId] UNIQUE ([UserId], [ChatRoomId])
    );

    -- Create index for faster queries
    CREATE INDEX [IX_ChatRoomMemberships_UserId_ChatRoomId] 
        ON [dbo].[ChatRoomMemberships]([UserId], [ChatRoomId]);
END
GO

PRINT 'Chat Feature Migration completed successfully';
