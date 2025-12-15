-- =============================================
-- Schema Synchronization Migration
-- =============================================
-- This script updates an existing database to match the current entity model schema
-- Run this on an existing database to apply all schema changes
-- Date: 2025-12-15
-- =============================================

USE [stjago-volleyball-demo];
GO

PRINT '=============================================';
PRINT 'Starting Schema Synchronization Migration';
PRINT '=============================================';
GO

-- =============================================
-- 1. Update Tournaments Table
-- =============================================
PRINT 'Updating Tournaments table...';

-- Add RegistrationUrl if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tournaments]') AND name = 'RegistrationUrl')
BEGIN
    ALTER TABLE dbo.Tournaments
    ADD RegistrationUrl NVARCHAR(MAX) NULL;
    
    PRINT '  - Added RegistrationUrl column';
END
ELSE
BEGIN
    PRINT '  - RegistrationUrl column already exists';
END
GO

-- Update CreatedBy and UpdatedBy to have defaults
IF EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Tournaments]') 
    AND name = 'CreatedBy' 
    AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.Tournaments ALTER COLUMN CreatedBy NVARCHAR(256) NOT NULL;
    ALTER TABLE dbo.Tournaments ADD DEFAULT 'system' FOR CreatedBy;
    PRINT '  - Updated CreatedBy column with NOT NULL and default';
END

IF EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Tournaments]') 
    AND name = 'UpdatedBy' 
    AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.Tournaments ALTER COLUMN UpdatedBy NVARCHAR(256) NOT NULL;
    ALTER TABLE dbo.Tournaments ADD DEFAULT 'system' FOR UpdatedBy;
    PRINT '  - Updated UpdatedBy column with NOT NULL and default';
END
GO

-- =============================================
-- 2. Update Rounds Table with Recommendations
-- =============================================
PRINT 'Updating Rounds table...';

-- Add RecommendedMatchGenerationStrategy if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Rounds]') AND name = 'RecommendedMatchGenerationStrategy')
BEGIN
    ALTER TABLE dbo.Rounds
    ADD RecommendedMatchGenerationStrategy NVARCHAR(50) NOT NULL DEFAULT 'RoundRobin';
    
    PRINT '  - Added RecommendedMatchGenerationStrategy column';
END
GO

-- Add RecommendedTeamSelectionStrategy if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Rounds]') AND name = 'RecommendedTeamSelectionStrategy')
BEGIN
    ALTER TABLE dbo.Rounds
    ADD RecommendedTeamSelectionStrategy NVARCHAR(50) NOT NULL DEFAULT 'TopByPoints';
    
    PRINT '  - Added RecommendedTeamSelectionStrategy column';
END
GO

-- Add IsPlayoff if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Rounds]') AND name = 'IsPlayoff')
BEGIN
    ALTER TABLE dbo.Rounds
    ADD IsPlayoff BIT NOT NULL DEFAULT 0;
    
    PRINT '  - Added IsPlayoff column';
END
GO

-- Rename QualifyingTeams to RecommendedQualifyingTeamsCount if needed
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Rounds]') AND name = 'QualifyingTeams')
AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Rounds]') AND name = 'RecommendedQualifyingTeamsCount')
BEGIN
    EXEC sp_rename 'dbo.Rounds.QualifyingTeams', 'RecommendedQualifyingTeamsCount', 'COLUMN';
    PRINT '  - Renamed QualifyingTeams to RecommendedQualifyingTeamsCount';
END
GO

-- =============================================
-- 3. Create/Update TournamentRounds Table
-- =============================================
PRINT 'Creating/Updating TournamentRounds table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TournamentRounds' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TournamentRounds (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TournamentId UNIQUEIDENTIFIER NOT NULL,
        DivisionId UNIQUEIDENTIFIER NOT NULL,
        RoundId UNIQUEIDENTIFIER NOT NULL,
        RoundNumber INT NOT NULL,
        MatchGenerationStrategy NVARCHAR(50) NOT NULL DEFAULT 'Manual',
        GroupingStrategy NVARCHAR(50) NOT NULL DEFAULT 'NoGroup',
        PreviousTournamentRoundId UNIQUEIDENTIFIER NULL,
        NextTournamentRoundId UNIQUEIDENTIFIER NULL,
        AdvancingTeamsCount INT NOT NULL DEFAULT 0,
        AdvancingTeamSelectionStrategy NVARCHAR(50) NOT NULL DEFAULT 'Manual',
        QualifyingTeamsCount INT NOT NULL DEFAULT 0,
        QualifyingTeamSelectionStrategy NVARCHAR(50) NOT NULL DEFAULT 'Manual',
        IsPlayoff BIT NOT NULL DEFAULT 0,
        IsFinished BIT NOT NULL DEFAULT 0,
        IsLocked BIT NOT NULL DEFAULT 0,
        TeamsPerGroup INT NULL,
        GroupsInRound INT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        
        CONSTRAINT FK_TournamentRounds_Tournament FOREIGN KEY (TournamentId) 
            REFERENCES dbo.Tournaments(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TournamentRounds_Division FOREIGN KEY (DivisionId) 
            REFERENCES dbo.Divisions(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_TournamentRounds_Round FOREIGN KEY (RoundId) 
            REFERENCES dbo.Rounds(Id) ON DELETE NO ACTION,
        CONSTRAINT CK_TournamentRounds_RoundNumber CHECK (RoundNumber >= 0),
        CONSTRAINT CK_TournamentRounds_AdvancingTeamsCount CHECK (AdvancingTeamsCount >= 0),
        CONSTRAINT CK_TournamentRounds_QualifyingTeamsCount CHECK (QualifyingTeamsCount >= 0)
    );
    
    CREATE INDEX IX_TournamentRounds_Tournament ON dbo.TournamentRounds(TournamentId);
    CREATE INDEX IX_TournamentRounds_Division ON dbo.TournamentRounds(DivisionId);
    CREATE INDEX IX_TournamentRounds_Round ON dbo.TournamentRounds(RoundId);
    CREATE INDEX IX_TournamentRounds_TournamentDivisionRound ON dbo.TournamentRounds(TournamentId, DivisionId, RoundId);
    CREATE INDEX IX_TournamentRounds_RoundNumber ON dbo.TournamentRounds(RoundNumber);
    CREATE INDEX IX_TournamentRounds_PreviousRound ON dbo.TournamentRounds(PreviousTournamentRoundId);
    
    PRINT '  - TournamentRounds table created';
END
ELSE
BEGIN
    PRINT '  - TournamentRounds table already exists, checking for missing columns...';
    
    -- Add TeamsPerGroup if missing
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TournamentRounds]') AND name = 'TeamsPerGroup')
    BEGIN
        ALTER TABLE dbo.TournamentRounds ADD TeamsPerGroup INT NULL;
        PRINT '    - Added TeamsPerGroup column';
    END
    
    -- Add GroupsInRound if missing
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TournamentRounds]') AND name = 'GroupsInRound')
    BEGIN
        ALTER TABLE dbo.TournamentRounds ADD GroupsInRound INT NULL;
        PRINT '    - Added GroupsInRound column';
    END
    
    -- Add QualifyingTeamsCount if missing
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TournamentRounds]') AND name = 'QualifyingTeamsCount')
    BEGIN
        ALTER TABLE dbo.TournamentRounds ADD QualifyingTeamsCount INT NOT NULL DEFAULT 0;
        PRINT '    - Added QualifyingTeamsCount column';
    END
    
    -- Add QualifyingTeamSelectionStrategy if missing
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TournamentRounds]') AND name = 'QualifyingTeamSelectionStrategy')
    BEGIN
        ALTER TABLE dbo.TournamentRounds ADD QualifyingTeamSelectionStrategy NVARCHAR(50) NOT NULL DEFAULT 'Manual';
        PRINT '    - Added QualifyingTeamSelectionStrategy column';
    END
    
    -- Add IsPlayoff if missing
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TournamentRounds]') AND name = 'IsPlayoff')
    BEGIN
        ALTER TABLE dbo.TournamentRounds ADD IsPlayoff BIT NOT NULL DEFAULT 0;
        PRINT '    - Added IsPlayoff column';
    END
    
    -- Rename columns if they have old names
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TournamentRounds]') AND name = 'TeamsAdvancing')
    AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TournamentRounds]') AND name = 'AdvancingTeamsCount')
    BEGIN
        EXEC sp_rename 'dbo.TournamentRounds.TeamsAdvancing', 'AdvancingTeamsCount', 'COLUMN';
        PRINT '    - Renamed TeamsAdvancing to AdvancingTeamsCount';
    END
    
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TournamentRounds]') AND name = 'TeamSelectionMethod')
    AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TournamentRounds]') AND name = 'AdvancingTeamSelectionStrategy')
    BEGIN
        EXEC sp_rename 'dbo.TournamentRounds.TeamSelectionMethod', 'AdvancingTeamSelectionStrategy', 'COLUMN';
        PRINT '    - Renamed TeamSelectionMethod to AdvancingTeamSelectionStrategy';
    END
END
GO

-- =============================================
-- 4. Create/Update TournamentRoundTeams Table
-- =============================================
PRINT 'Creating/Updating TournamentRoundTeams table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TournamentRoundTeams' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TournamentRoundTeams (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TournamentId UNIQUEIDENTIFIER NOT NULL,
        DivisionId UNIQUEIDENTIFIER NOT NULL,
        RoundId UNIQUEIDENTIFIER NOT NULL,
        TeamId UNIQUEIDENTIFIER NOT NULL,
        TournamentRoundId UNIQUEIDENTIFIER NOT NULL,
        SeedNumber INT NOT NULL DEFAULT 0,
        RankingPoints INT NOT NULL DEFAULT 0,
        Rank INT NOT NULL DEFAULT 0,
        Points INT NOT NULL DEFAULT 0,
        MatchesPlayed INT NOT NULL DEFAULT 0,
        Wins INT NOT NULL DEFAULT 0,
        Draws INT NOT NULL DEFAULT 0,
        Losses INT NOT NULL DEFAULT 0,
        SetsFor INT NOT NULL DEFAULT 0,
        SetsAgainst INT NOT NULL DEFAULT 0,
        ScoreFor INT NOT NULL DEFAULT 0,
        ScoreAgainst INT NOT NULL DEFAULT 0,
        GroupName NVARCHAR(100) NOT NULL DEFAULT '',
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        
        CONSTRAINT FK_TournamentRoundTeams_Tournament FOREIGN KEY (TournamentId) 
            REFERENCES dbo.Tournaments(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TournamentRoundTeams_Division FOREIGN KEY (DivisionId) 
            REFERENCES dbo.Divisions(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_TournamentRoundTeams_Round FOREIGN KEY (RoundId) 
            REFERENCES dbo.Rounds(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_TournamentRoundTeams_Team FOREIGN KEY (TeamId) 
            REFERENCES dbo.Teams(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_TournamentRoundTeams_TournamentRound FOREIGN KEY (TournamentRoundId) 
            REFERENCES dbo.TournamentRounds(Id) ON DELETE NO ACTION,
        CONSTRAINT UQ_TournamentRoundTeams_TournamentRoundTeam 
            UNIQUE (TournamentRoundId, TeamId),
        CONSTRAINT CK_TournamentRoundTeams_SeedNumber CHECK (SeedNumber >= 0),
        CONSTRAINT CK_TournamentRoundTeams_Points CHECK (Points >= 0),
        CONSTRAINT CK_TournamentRoundTeams_MatchesPlayed CHECK (MatchesPlayed >= 0),
        CONSTRAINT CK_TournamentRoundTeams_Wins CHECK (Wins >= 0),
        CONSTRAINT CK_TournamentRoundTeams_Draws CHECK (Draws >= 0),
        CONSTRAINT CK_TournamentRoundTeams_Losses CHECK (Losses >= 0),
        CONSTRAINT CK_TournamentRoundTeams_SetsFor CHECK (SetsFor >= 0),
        CONSTRAINT CK_TournamentRoundTeams_SetsAgainst CHECK (SetsAgainst >= 0),
        CONSTRAINT CK_TournamentRoundTeams_ScoreFor CHECK (ScoreFor >= 0),
        CONSTRAINT CK_TournamentRoundTeams_ScoreAgainst CHECK (ScoreAgainst >= 0)
    );
    
    CREATE INDEX IX_TournamentRoundTeams_Tournament ON dbo.TournamentRoundTeams(TournamentId);
    CREATE INDEX IX_TournamentRoundTeams_Division ON dbo.TournamentRoundTeams(DivisionId);
    CREATE INDEX IX_TournamentRoundTeams_Round ON dbo.TournamentRoundTeams(RoundId);
    CREATE INDEX IX_TournamentRoundTeams_Team ON dbo.TournamentRoundTeams(TeamId);
    CREATE INDEX IX_TournamentRoundTeams_TournamentRound ON dbo.TournamentRoundTeams(TournamentRoundId);
    CREATE INDEX IX_TournamentRoundTeams_SeedNumber ON dbo.TournamentRoundTeams(SeedNumber);
    CREATE INDEX IX_TournamentRoundTeams_Rank ON dbo.TournamentRoundTeams(Rank);
    CREATE INDEX IX_TournamentRoundTeams_Points ON dbo.TournamentRoundTeams(Points);
    
    PRINT '  - TournamentRoundTeams table created';
END
ELSE
BEGIN
    PRINT '  - TournamentRoundTeams table already exists';
END
GO

-- =============================================
-- 5. Create/Update MatchSets Table
-- =============================================
PRINT 'Creating/Updating MatchSets table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MatchSets' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.MatchSets (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        MatchId UNIQUEIDENTIFIER NOT NULL,
        SetNumber INT NOT NULL,
        HomeTeamScore INT NOT NULL DEFAULT 0,
        AwayTeamScore INT NOT NULL DEFAULT 0,
        IsFinished BIT NOT NULL DEFAULT 0,
        IsLocked BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        
        CONSTRAINT FK_MatchSets_Matches_MatchId FOREIGN KEY (MatchId) 
            REFERENCES dbo.Matches(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_MatchSets_MatchId_SetNumber UNIQUE (MatchId, SetNumber)
    );
    
    CREATE INDEX IX_MatchSets_MatchId ON dbo.MatchSets(MatchId);
    
    PRINT '  - MatchSets table created';
END
ELSE
BEGIN
    PRINT '  - MatchSets table already exists';
END
GO

-- =============================================
-- 6. Create/Update Announcements Table
-- =============================================
PRINT 'Creating/Updating Announcements table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Announcements' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Announcements
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        TournamentId UNIQUEIDENTIFIER NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        Priority NVARCHAR(20) NOT NULL DEFAULT 'Info',
        SequencingNumber INT NOT NULL DEFAULT 0,
        FirstAnnouncementTime DATETIME2 NULL,
        LastAnnouncementTime DATETIME2 NULL,
        RemainingRepeatCount INT NOT NULL DEFAULT 1,
        AnnouncedCount INT NOT NULL DEFAULT 0,
        IsHidden BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        CONSTRAINT FK_Announcements_Tournaments FOREIGN KEY (TournamentId) REFERENCES dbo.Tournaments(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_Announcements_SequencingNumber ON dbo.Announcements(SequencingNumber);
    CREATE INDEX IX_Announcements_Tournament ON dbo.Announcements(TournamentId);
    CREATE INDEX IX_Announcements_Priority ON dbo.Announcements(Priority);
    CREATE INDEX IX_Announcements_IsHidden ON dbo.Announcements(IsHidden);
    
    PRINT '  - Announcements table created';
END
ELSE
BEGIN
    PRINT '  - Announcements table already exists';
END
GO

-- =============================================
-- 7. Create/Update AnnouncementHistoryLogs Table
-- =============================================
PRINT 'Creating/Updating AnnouncementHistoryLogs table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AnnouncementHistoryLogs' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.AnnouncementHistoryLogs
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        AnnouncementId UNIQUEIDENTIFIER NOT NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        
        CONSTRAINT FK_AnnouncementHistoryLogs_Announcements FOREIGN KEY (AnnouncementId) 
            REFERENCES dbo.Announcements(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_AnnouncementHistoryLogs_AnnouncementId ON dbo.AnnouncementHistoryLogs(AnnouncementId);
    CREATE INDEX IX_AnnouncementHistoryLogs_Timestamp ON dbo.AnnouncementHistoryLogs(Timestamp);
    
    PRINT '  - AnnouncementHistoryLogs table created';
END
ELSE
BEGIN
    PRINT '  - AnnouncementHistoryLogs table already exists';
END
GO

-- =============================================
-- 8. Verify Foreign Key Constraints
-- =============================================
PRINT 'Verifying foreign key constraints...';

-- Check Matches foreign keys have correct DELETE behavior
IF EXISTS (
    SELECT * FROM sys.foreign_keys 
    WHERE name = 'FK_Matches_Tournaments' 
    AND delete_referential_action_desc != 'CASCADE'
)
BEGIN
    PRINT '  - NOTE: FK_Matches_Tournaments does not have CASCADE DELETE. Consider updating manually if needed.';
END

PRINT '  - Foreign key verification complete';
GO

PRINT '=============================================';
PRINT 'Schema Synchronization Migration Completed!';
PRINT 'Database schema is now synchronized with entity models.';
PRINT '=============================================';
GO
