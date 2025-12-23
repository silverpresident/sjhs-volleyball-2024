-- =============================================
-- ST JAGO Volleyball Rally Manager - Complete Database Setup
-- =============================================
-- This script creates all tables needed for the Volleyball Rally Manager
-- Run this script on a fresh database to create the complete schema

-- Create Schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'dbo')
BEGIN
    EXEC('CREATE SCHEMA dbo');
END
GO

-- =============================================
-- Core Tables
-- =============================================

-- Divisions
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Divisions' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Divisions
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(50) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system'
    );
    PRINT 'Divisions table created successfully.';
END
GO

-- Tournaments
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tournaments' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Tournaments
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        TournamentDate DATETIME2 NOT NULL,
        IsActive BIT NOT NULL DEFAULT 0,
        RegistrationUrl NVARCHAR(1000) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system'
    );
    PRINT 'Tournaments table created successfully.';
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
        LogoUrl NVARCHAR(500),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system'
    );
    PRINT 'Teams table created successfully.';
END
GO

-- RoundTemplates
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RoundTemplates' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.RoundTemplates
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Name NVARCHAR(50) NOT NULL,
        Sequence INT NOT NULL,
        RecommendedQualifyingTeamsCount INT NOT NULL DEFAULT 0,
        RecommendedMatchGenerationStrategy NVARCHAR(50) NOT NULL DEFAULT 'RoundRobin',
        RecommendedTeamSelectionStrategy NVARCHAR(50) NOT NULL DEFAULT 'TopByPoints',
        IsPlayoff BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system'
    );
    PRINT 'RoundTemplates table created successfully.';
END
GO

-- =============================================
-- Relationship Tables
-- =============================================

-- TournamentDivisions
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TournamentDivisions' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TournamentDivisions
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        TournamentId UNIQUEIDENTIFIER NOT NULL,
        DivisionId UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        CONSTRAINT FK_TournamentDivisions_Tournaments FOREIGN KEY (TournamentId) REFERENCES dbo.Tournaments(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TournamentDivisions_Divisions FOREIGN KEY (DivisionId) REFERENCES dbo.Divisions(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_TournamentDivisions_TournamentId_DivisionId UNIQUE (TournamentId, DivisionId)
    );
    PRINT 'TournamentDivisions table created successfully.';
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
        GroupName NVARCHAR(50) NULL,
        Rating INT NOT NULL DEFAULT 0,
        SeedNumber INT NOT NULL DEFAULT 0,
        RankingPoints INT NOT NULL DEFAULT 0,
        Rank INT NOT NULL DEFAULT 0,
        TotalPoints INT NOT NULL DEFAULT 0,
        MatchesPlayed INT NOT NULL DEFAULT 0,
        SetsFor INT NOT NULL DEFAULT 0,
        SetsAgainst INT NOT NULL DEFAULT 0,
        ScoreFor INT NOT NULL DEFAULT 0,
        ScoreAgainst INT NOT NULL DEFAULT 0,
        Wins INT NOT NULL DEFAULT 0,
        Draws INT NOT NULL DEFAULT 0,
        Losses INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        CONSTRAINT FK_TournamentTeamDivisions_Tournaments FOREIGN KEY (TournamentId) REFERENCES dbo.Tournaments(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TournamentTeamDivisions_Teams FOREIGN KEY (TeamId) REFERENCES dbo.Teams(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_TournamentTeamDivisions_Divisions FOREIGN KEY (DivisionId) REFERENCES dbo.Divisions(Id) ON DELETE NO ACTION
    );
    PRINT 'TournamentTeamDivisions table created successfully.';
END
GO

-- =============================================
-- Tournament Rounds Management Tables
-- =============================================

-- TournamentRounds
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TournamentRounds' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TournamentRounds (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TournamentId UNIQUEIDENTIFIER NOT NULL,
        DivisionId UNIQUEIDENTIFIER NOT NULL,
        RoundTemplateId UNIQUEIDENTIFIER NOT NULL,
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
        
        -- Foreign Key Constraints
        CONSTRAINT FK_TournamentRounds_Tournament FOREIGN KEY (TournamentId) 
            REFERENCES dbo.Tournaments(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TournamentRounds_Division FOREIGN KEY (DivisionId) 
            REFERENCES dbo.Divisions(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_TournamentRounds_RoundTemplate FOREIGN KEY (RoundTemplateId) 
            REFERENCES dbo.RoundTemplates(Id) ON DELETE NO ACTION,
        
        -- Check Constraints
        CONSTRAINT CK_TournamentRounds_RoundNumber CHECK (RoundNumber >= 0),
        CONSTRAINT CK_TournamentRounds_AdvancingTeamsCount CHECK (AdvancingTeamsCount >= 0),
        CONSTRAINT CK_TournamentRounds_QualifyingTeamsCount CHECK (QualifyingTeamsCount >= 0)
    );
    
    -- Create Indexes
    CREATE INDEX IX_TournamentRounds_Tournament ON dbo.TournamentRounds(TournamentId);
    CREATE INDEX IX_TournamentRounds_Division ON dbo.TournamentRounds(DivisionId);
    CREATE INDEX IX_TournamentRounds_RoundTemplate ON dbo.TournamentRounds(RoundTemplateId);
    CREATE INDEX IX_TournamentRounds_TournamentDivisionRound ON dbo.TournamentRounds(TournamentId, DivisionId, RoundNumber);
    CREATE INDEX IX_TournamentRounds_RoundNumber ON dbo.TournamentRounds(RoundNumber);
    CREATE INDEX IX_TournamentRounds_PreviousRound ON dbo.TournamentRounds(PreviousTournamentRoundId);
    
    PRINT 'TournamentRounds table created successfully.';
END
GO

-- TournamentRoundTeams
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TournamentRoundTeams' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TournamentRoundTeams (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TournamentId UNIQUEIDENTIFIER NOT NULL,
        DivisionId UNIQUEIDENTIFIER NOT NULL,
        TournamentRoundId UNIQUEIDENTIFIER NOT NULL,
        RoundTemplateId UNIQUEIDENTIFIER NOT NULL,
        TeamId UNIQUEIDENTIFIER NOT NULL,
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
        GroupName NVARCHAR(50) NOT NULL DEFAULT '',
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        
        -- Foreign Key Constraints
        CONSTRAINT FK_TournamentRoundTeams_Tournament FOREIGN KEY (TournamentId) 
            REFERENCES dbo.Tournaments(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TournamentRoundTeams_Division FOREIGN KEY (DivisionId) 
            REFERENCES dbo.Divisions(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_TournamentRoundTeams_RoundTemplate FOREIGN KEY (RoundTemplateId) 
            REFERENCES dbo.RoundTemplates(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_TournamentRoundTeams_Team FOREIGN KEY (TeamId) 
            REFERENCES dbo.Teams(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_TournamentRoundTeams_TournamentRound FOREIGN KEY (TournamentRoundId) 
            REFERENCES dbo.TournamentRounds(Id) ON DELETE NO ACTION,
        
        -- Unique Constraint: One team per tournament round
        CONSTRAINT UQ_TournamentRoundTeams_TournamentRoundTeam 
            UNIQUE (TournamentRoundId, TeamId),
        
        -- Check Constraints
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
    
    -- Create Indexes
    CREATE INDEX IX_TournamentRoundTeams_Tournament ON dbo.TournamentRoundTeams(TournamentId);
    CREATE INDEX IX_TournamentRoundTeams_Division ON dbo.TournamentRoundTeams(DivisionId);
    CREATE INDEX IX_TournamentRoundTeams_Team ON dbo.TournamentRoundTeams(TeamId);
    CREATE INDEX IX_TournamentRoundTeams_TournamentRound ON dbo.TournamentRoundTeams(TournamentRoundId);
    CREATE INDEX IX_TournamentRoundTeams_SeedNumber ON dbo.TournamentRoundTeams(SeedNumber);
    CREATE INDEX IX_TournamentRoundTeams_Rank ON dbo.TournamentRoundTeams(Rank);
    CREATE INDEX IX_TournamentRoundTeams_Points ON dbo.TournamentRoundTeams(Points);
    
    PRINT 'TournamentRoundTeams table created successfully.';
END
GO

-- =============================================
-- Match Management Tables
-- =============================================

-- Matches
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Matches' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Matches (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        MatchNumber INT NOT NULL,
        ScheduledTime DATETIME2 NOT NULL,
        ActualStartTime DATETIME2,
        CourtLocation NVARCHAR(50) NOT NULL,
        GroupName NVARCHAR(50) NOT NULL,
        TournamentId UNIQUEIDENTIFIER NOT NULL,
        DivisionId UNIQUEIDENTIFIER NOT NULL,
        TournamentRoundId UNIQUEIDENTIFIER NOT NULL,
        RoundTemplateId UNIQUEIDENTIFIER NOT NULL,
        HomeTeamId UNIQUEIDENTIFIER NOT NULL,
        AwayTeamId UNIQUEIDENTIFIER NOT NULL,
        HomeTeamScore INT NOT NULL DEFAULT 0,
        AwayTeamScore INT NOT NULL DEFAULT 0,
        CurrentSetNumber INT NOT NULL DEFAULT 0,
        IsFinished BIT NOT NULL DEFAULT 0,
        IsLocked BIT NOT NULL DEFAULT 0,
        IsDisputed BIT NOT NULL DEFAULT 0,
        RefereeName NVARCHAR(255),
        ScorerName NVARCHAR(255),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        CONSTRAINT FK_Matches_Tournaments FOREIGN KEY (TournamentId) REFERENCES dbo.Tournaments(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Matches_Divisions FOREIGN KEY (DivisionId) REFERENCES dbo.Divisions(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Matches_TournamentRounds FOREIGN KEY (TournamentRoundId) REFERENCES dbo.TournamentRounds(Id),
        CONSTRAINT FK_Matches_RoundTemplates FOREIGN KEY (RoundTemplateId) REFERENCES dbo.RoundTemplates(Id),
        CONSTRAINT FK_Matches_HomeTeam FOREIGN KEY (HomeTeamId) REFERENCES dbo.Teams(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Matches_AwayTeam FOREIGN KEY (AwayTeamId) REFERENCES dbo.Teams(Id) ON DELETE NO ACTION
    );
    
    -- Create Indexes
    CREATE INDEX IX_Matches_Tournament ON dbo.Matches(TournamentId);
    CREATE INDEX IX_Matches_Division ON dbo.Matches(DivisionId);
    CREATE INDEX IX_Matches_TournamentRound ON dbo.Matches(TournamentRoundId);
    CREATE INDEX IX_Matches_Teams ON dbo.Matches(HomeTeamId, AwayTeamId);
    
    PRINT 'Matches table created successfully.';
END
GO

-- MatchSets
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
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        
        -- Foreign key constraint
        CONSTRAINT FK_MatchSets_Matches_MatchId FOREIGN KEY (MatchId) 
            REFERENCES dbo.Matches(Id) ON DELETE CASCADE,
        
        -- Unique constraint on MatchId and SetNumber combination
        CONSTRAINT UQ_MatchSets_MatchId_SetNumber UNIQUE (MatchId, SetNumber)
    );
    
    -- Create index for better query performance
    CREATE INDEX IX_MatchSets_MatchId ON dbo.MatchSets(MatchId);
    
    PRINT 'MatchSets table created successfully.';
END
GO

-- MatchUpdates
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
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        CONSTRAINT FK_MatchUpdates_Matches FOREIGN KEY (MatchId) REFERENCES dbo.Matches(Id) ON DELETE CASCADE
    );
    
    -- Create Index
    CREATE INDEX IX_MatchUpdates_Match ON dbo.MatchUpdates(MatchId);
    
    PRINT 'MatchUpdates table created successfully.';
END
GO

-- =============================================
-- Communication Tables
-- =============================================

-- Bulletins
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Bulletins' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Bulletins
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Content NVARCHAR(MAX) NOT NULL,
        RenderedContent NVARCHAR(MAX) NOT NULL,
        UseMarkdown BIT NOT NULL DEFAULT 0,
        Priority NVARCHAR(20) NOT NULL,
        IsVisible BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system'
    );
    
    -- Create Index
    CREATE INDEX IX_Bulletins_Priority ON dbo.Bulletins(Priority);
    
    PRINT 'Bulletins table created successfully.';
END
GO

-- Announcements
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
        Tag NVARCHAR(256) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        CONSTRAINT FK_Announcements_Tournaments FOREIGN KEY (TournamentId) REFERENCES dbo.Tournaments(Id) ON DELETE CASCADE
    );
    
    -- Create Indexes
    CREATE INDEX IX_Announcements_SequencingNumber ON dbo.Announcements(SequencingNumber);
    CREATE INDEX IX_Announcements_Tournament ON dbo.Announcements(TournamentId);
    CREATE INDEX IX_Announcements_Priority ON dbo.Announcements(Priority);
    CREATE INDEX IX_Announcements_IsHidden ON dbo.Announcements(IsHidden);
    CREATE INDEX IX_Announcements_Tag ON dbo.Announcements(Tag) WHERE Tag IS NOT NULL;
    
    PRINT 'Announcements table created successfully.';
END
GO

-- AnnouncementHistoryLogs
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
        
        -- Foreign key constraint
        CONSTRAINT FK_AnnouncementHistoryLogs_Announcements FOREIGN KEY (AnnouncementId) 
            REFERENCES dbo.Announcements(Id) ON DELETE CASCADE
    );
    
    -- Create Indexes
    CREATE INDEX IX_AnnouncementHistoryLogs_AnnouncementId ON dbo.AnnouncementHistoryLogs(AnnouncementId);
    CREATE INDEX IX_AnnouncementHistoryLogs_Timestamp ON dbo.AnnouncementHistoryLogs(Timestamp);
    
    PRINT 'AnnouncementHistoryLogs table created successfully.';
END
GO

-- =============================================
-- Chat Feature Tables
-- =============================================

-- ChatRooms
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatRooms' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.ChatRooms (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        RoomType NVARCHAR(50) NOT NULL,
        RequiredRole NVARCHAR(50) NULL,
        IsSystemRoom BIT NOT NULL DEFAULT 0,
        OwnerId NVARCHAR(450) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system'
    );
    PRINT 'ChatRooms table created successfully.';
END
GO

-- ChatMessages
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatMessages' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.ChatMessages (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ChatRoomId UNIQUEIDENTIFIER NOT NULL,
        SenderId NVARCHAR(450) NOT NULL,
        SenderName NVARCHAR(100) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT GETDATE(),
        IsRead BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        CONSTRAINT FK_ChatMessages_ChatRooms FOREIGN KEY (ChatRoomId) 
            REFERENCES dbo.ChatRooms(Id) ON DELETE CASCADE
    );

    -- Create index for faster queries
    CREATE INDEX IX_ChatMessages_ChatRoomId_Timestamp 
        ON dbo.ChatMessages(ChatRoomId, Timestamp);
    PRINT 'ChatMessages table created successfully.';
END
GO

-- ChatRoomMemberships
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatRoomMemberships' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.ChatRoomMemberships (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ChatRoomId UNIQUEIDENTIFIER NOT NULL,
        UserId NVARCHAR(450) NOT NULL,
        IsRoomAdmin BIT NOT NULL DEFAULT 0,
        IsMuted BIT NOT NULL DEFAULT 0,
        JoinedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        LastReadAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        CONSTRAINT FK_ChatRoomMemberships_ChatRooms FOREIGN KEY (ChatRoomId) 
            REFERENCES dbo.ChatRooms(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_ChatRoomMemberships_UserId_ChatRoomId UNIQUE (UserId, ChatRoomId)
    );

    -- Create index for faster queries
    CREATE INDEX IX_ChatRoomMemberships_UserId_ChatRoomId 
        ON dbo.ChatRoomMemberships(UserId, ChatRoomId);
    PRINT 'ChatRoomMemberships table created successfully.';
END
GO

-- =============================================
-- Insert Initial Data
-- =============================================

-- Insert Divisions
IF NOT EXISTS (SELECT * FROM dbo.Divisions)
BEGIN
    INSERT INTO dbo.Divisions (Id, Name, CreatedBy, UpdatedBy) VALUES
    (NEWID(), 'BOYS', 'system', 'system'),
    (NEWID(), 'GIRLS', 'system', 'system');
    
    PRINT 'Initial Divisions data inserted successfully.';
END
GO

-- Insert Rounds with updated recommendations
IF NOT EXISTS (SELECT * FROM dbo.RoundTemplates)
BEGIN
    INSERT INTO dbo.RoundTemplates (Id, Name, Sequence, RecommendedQualifyingTeamsCount, RecommendedMatchGenerationStrategy, RecommendedTeamSelectionStrategy, IsPlayoff, CreatedBy, UpdatedBy) VALUES
    (NEWID(), 'Preliminary Round', 1, 0, 'RoundRobin', 'TopFromGroupAndNextBest', 0, 'system', 'system'),
    (NEWID(), 'Seeded Round', 2, 16, 'GroupStageKnockout', 'TopByPoints', 0, 'system', 'system'),
    (NEWID(), 'Quarter Finals', 4, 8, 'SeededBracket', 'TopFromGroupAndNextBest', 0, 'system', 'system'),
    (NEWID(), 'Semi-finals', 5, 4, 'SeededBracket', 'WinnersOnly', 0, 'system', 'system'),
    (NEWID(), 'Third Place Playoff', 6, 2, 'SeededBracket', 'WinnersOnly', 1, 'system', 'system'),
    (NEWID(), 'Finals', 7, 2, 'SeededBracket', 'WinnersOnly', 0, 'system', 'system');
    
    PRINT 'Initial RoundTemplates data inserted successfully.';
END
GO

PRINT '=============================================';
PRINT 'Database setup completed successfully!';
PRINT 'All tables have been created with proper relationships and indexes.';
PRINT '=============================================';
GO
