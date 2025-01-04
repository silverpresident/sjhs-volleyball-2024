-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'stjago-volleyball-demo')
BEGIN
    CREATE DATABASE stjago-volleyball-demo;
END
GO

USE stjago-volleyball-demo;
GO

-- Create Schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'dbo')
BEGIN
    EXEC('CREATE SCHEMA dbo');
END
GO

-- Create Tables
-- Divisions
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Divisions' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Divisions
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
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Teams' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Teams
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Name NVARCHAR(100) NOT NULL,
        School NVARCHAR(100) NOT NULL,
        Color NVARCHAR(50) NOT NULL,
        DivisionId UNIQUEIDENTIFIER NOT NULL,
        LogoUrl NVARCHAR(500),
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
        CONSTRAINT FK_Teams_Divisions FOREIGN KEY (DivisionId) REFERENCES dbo.Divisions(Id)
    );
END
GO

-- Rounds
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Rounds' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Rounds
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
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
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Matches' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    -- Table: Matches
    CREATE TABLE dbo.Matches (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        MatchNumber INT NOT NULL,
        ScheduledTime DATETIME2 NOT NULL,
        ActualStartTime DATETIME2,
        CourtLocation NVARCHAR(50) NOT NULL,
        TournamentId UNIQUEIDENTIFIER NOT NULL,
        DivisionId UNIQUEIDENTIFIER NOT NULL,
        RoundId UNIQUEIDENTIFIER NOT NULL,
        HomeTeamId UNIQUEIDENTIFIER NOT NULL,
        AwayTeamId UNIQUEIDENTIFIER NOT NULL,
        HomeTeamScore INT NOT NULL DEFAULT 0,
        AwayTeamScore INT NOT NULL DEFAULT 0,
        IsFinished BIT NOT NULL DEFAULT 0,
        IsDisputed BIT NOT NULL DEFAULT 0,
        RefereeName NVARCHAR(255),
        ScorerName NVARCHAR(255),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(256),
        UpdatedBy NVARCHAR(256),
        CONSTRAINT FK_Matches_Tournaments FOREIGN KEY (TournamentId) REFERENCES dbo.Tournaments(Id),
        CONSTRAINT FK_Matches_Divisions FOREIGN KEY (DivisionId) REFERENCES dbo.Divisions(Id),
        CONSTRAINT FK_Matches_Rounds FOREIGN KEY (RoundId) REFERENCES dbo.Rounds(Id),
        CONSTRAINT FK_Matches_HomeTeam FOREIGN KEY (HomeTeamId) REFERENCES dbo.Teams(Id),
        CONSTRAINT FK_Matches_AwayTeam FOREIGN KEY (AwayTeamId) REFERENCES dbo.Teams(Id)
    );
END
GO

-- Match Updates
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MatchUpdates' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.MatchUpdates
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        MatchId UNIQUEIDENTIFIER NOT NULL,
        UpdateType NVARCHAR(50) NOT NULL,
        Content NVARCHAR(500) NOT NULL,
        PreviousValue NVARCHAR(100),
        NewValue NVARCHAR(100),
        IsProcessed BIT NOT NULL DEFAULT 0,
        ProcessedAt DATETIME2,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(256),
        UpdatedBy NVARCHAR(256),
        CONSTRAINT FK_MatchUpdates_Matches FOREIGN KEY (MatchId) REFERENCES dbo.Matches(Id)
    );
END
GO

-- Announcements
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Announcements' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Announcements
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
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
IF NOT EXISTS (SELECT * FROM dbo.Divisions)
BEGIN
    INSERT INTO dbo.Divisions (Id, Name, CreatedBy, UpdatedBy) VALUES
    (NEWID(), 'BOYS', 'system', 'system'),
    (NEWID(), 'GIRLS', 'system', 'system');
END
GO

-- Insert Rounds
IF NOT EXISTS (SELECT * FROM dbo.Rounds)
BEGIN
    INSERT INTO dbo.Rounds (Id, Name, Sequence, CreatedBy, UpdatedBy) VALUES
    (NEWID(), 'Round 1', 1, 'system', 'system'),
    (NEWID(), 'Round 2', 2, 'system', 'system'),
    (NEWID(), 'Round 3', 3, 'system', 'system'),
    (NEWID(), 'Quarter Finals', 4, 'system', 'system'),
    (NEWID(), 'Semi Finals', 5, 'system', 'system'),
    (NEWID(), 'Finals', 6, 'system', 'system');
END
GO

-- Create Indexes
CREATE NONCLUSTERED INDEX IX_Teams_Division ON dbo.Teams(DivisionId);
CREATE NONCLUSTERED INDEX IX_Matches_Tournament ON dbo.Matches(TournamentId);
CREATE NONCLUSTERED INDEX IX_Matches_Division ON dbo.Matches(DivisionId);
CREATE NONCLUSTERED INDEX IX_Matches_Round ON dbo.Matches(RoundId);
CREATE NONCLUSTERED INDEX IX_Matches_Teams ON dbo.Matches(HomeTeamId, AwayTeamId);
CREATE NONCLUSTERED INDEX IX_MatchUpdates_Match ON dbo.MatchUpdates(MatchId);
CREATE NONCLUSTERED INDEX IX_Announcements_Priority ON dbo.Announcements(Priority);
GO

-- Tournaments
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tournaments' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Tournaments
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Name NVARCHAR(200) NOT NULL,
        StartDate DATETIME2 NOT NULL,
        EndDate DATETIME2 NOT NULL,
        IsActive BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(256),
        UpdatedBy NVARCHAR(256)
    );
END
GO

-- TournamentDivisions
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TournamentDivisions' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TournamentDivisions
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        TournamentId UNIQUEIDENTIFIER NOT NULL,
        DivisionId UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(256),
        UpdatedBy NVARCHAR(256),
        CONSTRAINT FK_TournamentDivisions_Tournaments FOREIGN KEY (TournamentId) REFERENCES dbo.Tournaments(Id),
        CONSTRAINT FK_TournamentDivisions_Divisions FOREIGN KEY (DivisionId) REFERENCES dbo.Divisions(Id)
    );
END
GO

-- TournamentTeamDivisions
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TournamentTeamDivisions' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TournamentTeamDivisions
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        TournamentId UNIQUEIDENTIFIER NOT NULL,
        TeamId UNIQUEIDENTIFIER NOT NULL,
        DivisionId UNIQUEIDENTIFIER NOT NULL,
        GroupName NVARCHAR(50),
        SeedNumber INT,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(256),
        UpdatedBy NVARCHAR(256),
        CONSTRAINT FK_TournamentTeamDivisions_Tournaments FOREIGN KEY (TournamentId) REFERENCES dbo.Tournaments(Id),
        CONSTRAINT FK_TournamentTeamDivisions_Teams FOREIGN KEY (TeamId) REFERENCES dbo.Teams(Id),
        CONSTRAINT FK_TournamentTeamDivisions_Divisions FOREIGN KEY (DivisionId) REFERENCES dbo.Divisions(Id)
    );
END
GO

-- Create Triggers for UpdatedAt
CREATE TRIGGER dbo.TR_Teams_UpdatedAt ON dbo.Teams AFTER UPDATE AS
BEGIN
    UPDATE dbo.Teams
    SET UpdatedAt = GETUTCDATE()
    FROM dbo.Teams t
    INNER JOIN inserted i ON t.Id = i.Id;
END
GO

CREATE TRIGGER dbo.TR_Matches_UpdatedAt ON dbo.Matches AFTER UPDATE AS
BEGIN
    UPDATE dbo.Matches
    SET UpdatedAt = GETUTCDATE()
    FROM dbo.Matches m
    INNER JOIN inserted i ON m.Id = i.Id;
END
GO

CREATE TRIGGER dbo.TR_MatchUpdates_UpdatedAt ON dbo.MatchUpdates AFTER UPDATE AS
BEGIN
    UPDATE dbo.MatchUpdates
    SET UpdatedAt = GETUTCDATE()
    FROM dbo.MatchUpdates mu
    INNER JOIN inserted i ON mu.Id = i.Id;
END
GO

CREATE TRIGGER dbo.TR_Announcements_UpdatedAt ON dbo.Announcements AFTER UPDATE AS
BEGIN
    UPDATE dbo.Announcements
    SET UpdatedAt = GETUTCDATE()
    FROM dbo.Announcements a
    INNER JOIN inserted i ON a.Id = i.Id;
END
GO
