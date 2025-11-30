-- Announcements Setup Script
-- This script creates the Announcements and AnnouncementHistoryLogs tables

-- Create Announcements table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Announcements' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Announcements
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Title NVARCHAR(200) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        Priority NVARCHAR(20) NOT NULL DEFAULT 'Info',
        SequencingNumber INT NOT NULL DEFAULT 0,
        FirstAnnouncementTime DATETIME2 NULL,
        LastAnnouncementTime DATETIME2 NULL,
        RemainingRepeatCount INT NOT NULL DEFAULT 1,
        AnnouncedCount INT NOT NULL DEFAULT 0,
        IsHidden BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(256),
        UpdatedBy NVARCHAR(256) 
    );
    
    PRINT 'Announcements table created successfully.';
END
ELSE
BEGIN
    PRINT 'Announcements table already exists.';
END
GO

-- Create AnnouncementHistoryLogs table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AnnouncementHistoryLogs' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.AnnouncementHistoryLogs
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        AnnouncementId UNIQUEIDENTIFIER NOT NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(256),
        UpdatedBy NVARCHAR(256),
        
        -- Foreign key constraint
        CONSTRAINT FK_AnnouncementHistoryLogs_Announcements FOREIGN KEY (AnnouncementId) 
            REFERENCES dbo.Announcements(Id) ON DELETE CASCADE
    );
    
    PRINT 'AnnouncementHistoryLogs table created successfully.';
END
ELSE
BEGIN
    PRINT 'AnnouncementHistoryLogs table already exists.';
END
GO

-- Create indexes for better query performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Announcements_SequencingNumber' AND object_id = OBJECT_ID('dbo.Announcements'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Announcements_SequencingNumber ON dbo.Announcements(SequencingNumber);
    PRINT 'Index IX_Announcements_SequencingNumber created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Announcements_Priority' AND object_id = OBJECT_ID('dbo.Announcements'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Announcements_Priority ON dbo.Announcements(Priority);
    PRINT 'Index IX_Announcements_Priority created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Announcements_IsHidden' AND object_id = OBJECT_ID('dbo.Announcements'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Announcements_IsHidden ON dbo.Announcements(IsHidden);
    PRINT 'Index IX_Announcements_IsHidden created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AnnouncementHistoryLogs_AnnouncementId' AND object_id = OBJECT_ID('dbo.AnnouncementHistoryLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AnnouncementHistoryLogs_AnnouncementId ON dbo.AnnouncementHistoryLogs(AnnouncementId);
    PRINT 'Index IX_AnnouncementHistoryLogs_AnnouncementId created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AnnouncementHistoryLogs_Timestamp' AND object_id = OBJECT_ID('dbo.AnnouncementHistoryLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AnnouncementHistoryLogs_Timestamp ON dbo.AnnouncementHistoryLogs(Timestamp);
    PRINT 'Index IX_AnnouncementHistoryLogs_Timestamp created successfully.';
END
GO

PRINT 'Announcements setup completed successfully.';
GO
