-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'VolleyballRally')
BEGIN
    CREATE DATABASE VolleyballRally;
END
GO

USE VolleyballRally;
GO

-- Create Schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'vb')
BEGIN
    EXEC('CREATE SCHEMA vb');
END
GO

-- Create Tables
-- Divisions
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Divisions' AND schema_id = SCHEMA_ID('vb'))
BEGIN
    CREATE TABLE vb.Divisions
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(50) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(256),
        UpdatedBy NVARCHAR(256)
    );
END
GO

-- Teams
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Teams' AND schema_id = SCHEMA_ID('vb'))
BEGIN
    CREATE TABLE vb.Teams
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(100) NOT NULL,
        School NVARCHAR(100) NOT NULL,
        Color NVARCHAR(7) NOT NULL,
        DivisionId UNIQUEIDENTIFIER NOT NULL,
        LogoUrl NVARCHAR(500),
        Points INT NOT NULL DEFAULT 0,
        PointsScored INT NOT NULL DEFAULT 0,
        PointsConceded INT NOT NULL DEFAULT 0,
        MatchesPlayed INT NOT NULL DEFAULT 0,
        Wins INT NOT NULL DEFAULT 0,
        Draws INT NOT NULL DEFAULT 0,
        Losses INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(256),
        UpdatedBy NVARCHAR(256),
        CONSTRAINT FK_Teams_Divisions FOREIGN KEY (DivisionId) REFERENCES vb.Divisions(Id)
    );
END
GO

-- Rounds
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Rounds' AND schema_id = SCHEMA_ID('vb'))
BEGIN
    CREATE TABLE vb.Rounds
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(50) NOT NULL,
        Sequence INT NOT NULL,
        IsComplete BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(256),
        UpdatedBy NVARCHAR(256)
    );
END
GO

-- Matches
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Matches' AND schema_id = SCHEMA_ID('vb'))
BEGIN
    CREATE TABLE vb.Matches
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        MatchNumber INT NOT NULL,
        RoundId UNIQUEIDENTIFIER NOT NULL,
        HomeTeamId UNIQUEIDENTIFIER NOT NULL,
        AwayTeamId UNIQUEIDENTIFIER NOT NULL,
        HomeTeamScore INT NOT NULL DEFAULT 0,
        AwayTeamScore INT NOT NULL DEFAULT 0,
        ScheduledTime DATETIME2 NOT NULL,
        ActualStartTime DATETIME2,
        CourtLocation NVARCHAR(50) NOT NULL,
        RefereeName NVARCHAR(100),
        ScorerName NVARCHAR(100),
        IsFinished BIT NOT NULL DEFAULT 0,
        IsDisputed BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(256),
        UpdatedBy NVARCHAR(256),
        CONSTRAINT FK_Matches_Rounds FOREIGN KEY (RoundId) REFERENCES vb.Rounds(Id),
        CONSTRAINT FK_Matches_HomeTeam FOREIGN KEY (HomeTeamId) REFERENCES vb.Teams(Id),
        CONSTRAINT FK_Matches_AwayTeam FOREIGN KEY (AwayTeamId) REFERENCES vb.Teams(Id)
    );
END
GO

-- Match Updates
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MatchUpdates' AND schema_id = SCHEMA_ID('vb'))
BEGIN
    CREATE TABLE vb.MatchUpdates
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        MatchId UNIQUEIDENTIFIER NOT NULL,
        UpdateType NVARCHAR(50) NOT NULL,
        UpdateText NVARCHAR(500) NOT NULL,
        PreviousValue NVARCHAR(100),
        NewValue NVARCHAR(100),
        IsProcessed BIT NOT NULL DEFAULT 0,
        ProcessedAt DATETIME2,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(256),
        UpdatedBy NVARCHAR(256),
        CONSTRAINT FK_MatchUpdates_Matches FOREIGN KEY (MatchId) REFERENCES vb.Matches(Id)
    );
END
GO

-- Announcements
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Announcements' AND schema_id = SCHEMA_ID('vb'))
BEGIN
    CREATE TABLE vb.Announcements
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Text NVARCHAR(MAX) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        RenderedContent NVARCHAR(MAX) NOT NULL,
        UseMarkdown BIT NOT NULL DEFAULT 0,
        Priority NVARCHAR(20) NOT NULL,
        IsVisible BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(256),
        UpdatedBy NVARCHAR(256)
    );
END
GO

-- Insert Initial Data
-- Insert Divisions
IF NOT EXISTS (SELECT * FROM vb.Divisions)
BEGIN
    INSERT INTO vb.Divisions (Id, Name) VALUES
    (NEWID(), 'BOYS'),
    (NEWID(), 'GIRLS');
END
GO

-- Insert Rounds
IF NOT EXISTS (SELECT * FROM vb.Rounds)
BEGIN
    INSERT INTO vb.Rounds (Id, Name, Sequence) VALUES
    (NEWID(), 'Round 1', 1),
    (NEWID(), 'Round 2', 2),
    (NEWID(), 'Round 3', 3),
    (NEWID(), 'Quarter Finals', 4),
    (NEWID(), 'Semi Finals', 5),
    (NEWID(), 'Finals', 6);
END
GO

-- Create Indexes
CREATE NONCLUSTERED INDEX IX_Teams_Division ON vb.Teams(DivisionId);
CREATE NONCLUSTERED INDEX IX_Matches_Round ON vb.Matches(RoundId);
CREATE NONCLUSTERED INDEX IX_Matches_Teams ON vb.Matches(HomeTeamId, AwayTeamId);
CREATE NONCLUSTERED INDEX IX_MatchUpdates_Match ON vb.MatchUpdates(MatchId);
CREATE NONCLUSTERED INDEX IX_Announcements_Priority ON vb.Announcements(Priority);
GO

-- Create Triggers for UpdatedAt
CREATE TRIGGER vb.TR_Teams_UpdatedAt ON vb.Teams AFTER UPDATE AS
BEGIN
    UPDATE vb.Teams
    SET UpdatedAt = GETUTCDATE()
    FROM vb.Teams t
    INNER JOIN inserted i ON t.Id = i.Id;
END
GO

CREATE TRIGGER vb.TR_Matches_UpdatedAt ON vb.Matches AFTER UPDATE AS
BEGIN
    UPDATE vb.Matches
    SET UpdatedAt = GETUTCDATE()
    FROM vb.Matches m
    INNER JOIN inserted i ON m.Id = i.Id;
END
GO

CREATE TRIGGER vb.TR_MatchUpdates_UpdatedAt ON vb.MatchUpdates AFTER UPDATE AS
BEGIN
    UPDATE vb.MatchUpdates
    SET UpdatedAt = GETUTCDATE()
    FROM vb.MatchUpdates mu
    INNER JOIN inserted i ON mu.Id = i.Id;
END
GO

CREATE TRIGGER vb.TR_Announcements_UpdatedAt ON vb.Announcements AFTER UPDATE AS
BEGIN
    UPDATE vb.Announcements
    SET UpdatedAt = GETUTCDATE()
    FROM vb.Announcements a
    INNER JOIN inserted i ON a.Id = i.Id;
END
GO
