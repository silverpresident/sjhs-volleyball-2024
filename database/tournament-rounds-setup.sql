-- =============================================
-- Tournament Rounds Management Schema Setup
-- =============================================
-- This script creates the tables needed for the Tournament Round Management feature
-- Run this script to add the TournamentRounds and TournamentRoundTeams tables

-- =============================================
-- Create TournamentRounds Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TournamentRounds')
BEGIN
    CREATE TABLE TournamentRounds (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TournamentId UNIQUEIDENTIFIER NOT NULL,
        DivisionId UNIQUEIDENTIFIER NOT NULL,
        RoundId UNIQUEIDENTIFIER NOT NULL,
        RoundNumber INT NOT NULL,
        TeamSelectionMethod NVARCHAR(50) NOT NULL DEFAULT 'Manual',
        MatchGenerationStrategy NVARCHAR(50) NOT NULL DEFAULT 'Manual',
        PreviousTournamentRoundId UNIQUEIDENTIFIER NULL,
        TeamsAdvancing INT NOT NULL DEFAULT 0,
        IsFinished BIT NOT NULL DEFAULT 0,
        IsLocked BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        
        -- Foreign Key Constraints
        CONSTRAINT FK_TournamentRounds_Tournament FOREIGN KEY (TournamentId) 
            REFERENCES Tournaments(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TournamentRounds_Division FOREIGN KEY (DivisionId) 
            REFERENCES Divisions(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_TournamentRounds_Round FOREIGN KEY (RoundId) 
            REFERENCES Rounds(Id) ON DELETE NO ACTION,
        
        -- Unique Constraint: One round per tournament-division-round combination
        CONSTRAINT UQ_TournamentRounds_TournamentDivisionRound 
            UNIQUE (TournamentId, DivisionId, RoundId),
        
        -- Check Constraints
        CONSTRAINT CK_TournamentRounds_RoundNumber CHECK (RoundNumber > 0),
        CONSTRAINT CK_TournamentRounds_TeamsAdvancing CHECK (TeamsAdvancing >= 0),
    );
    
    -- Create Indexes
    CREATE INDEX IX_TournamentRounds_Tournament ON TournamentRounds(TournamentId);
    CREATE INDEX IX_TournamentRounds_Division ON TournamentRounds(DivisionId);
    CREATE INDEX IX_TournamentRounds_Round ON TournamentRounds(RoundId);
    CREATE INDEX IX_TournamentRounds_TournamentDivisionRound ON TournamentRounds(TournamentId, DivisionId, RoundId);
    CREATE INDEX IX_TournamentRounds_RoundNumber ON TournamentRounds(RoundNumber);
    CREATE INDEX IX_TournamentRounds_PreviousRound ON TournamentRounds(PreviousTournamentRoundId);
    
    PRINT 'TournamentRounds table created successfully.';
END
ELSE
BEGIN
    PRINT 'TournamentRounds table already exists.';
END
GO

-- =============================================
-- Create TournamentRoundTeams Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TournamentRoundTeams')
BEGIN
    CREATE TABLE TournamentRoundTeams (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TournamentId UNIQUEIDENTIFIER NOT NULL,
        DivisionId UNIQUEIDENTIFIER NOT NULL,
        RoundId UNIQUEIDENTIFIER NOT NULL,
        TeamId UNIQUEIDENTIFIER NOT NULL,
        TournamentRoundId UNIQUEIDENTIFIER NOT NULL,
        SeedNumber INT NOT NULL DEFAULT 0,
        FinalRank INT NOT NULL DEFAULT 0,
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
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedBy NVARCHAR(256) NOT NULL DEFAULT 'system',
        
        -- Foreign Key Constraints
        CONSTRAINT FK_TournamentRoundTeams_Tournament FOREIGN KEY (TournamentId) 
            REFERENCES Tournaments(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TournamentRoundTeams_Division FOREIGN KEY (DivisionId) 
            REFERENCES Divisions(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_TournamentRoundTeams_Round FOREIGN KEY (RoundId) 
            REFERENCES Rounds(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_TournamentRoundTeams_Team FOREIGN KEY (TeamId) 
            REFERENCES Teams(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_TournamentRoundTeams_TournamentRound FOREIGN KEY (TournamentRoundId) 
            REFERENCES TournamentRounds(Id) ON DELETE NO ACTION,
        
        -- Unique Constraint: One team per tournament round
        CONSTRAINT UQ_TournamentRoundTeams_TournamentRoundTeam 
            UNIQUE (TournamentRoundId, TeamId),
        
        -- Check Constraints
        CONSTRAINT CK_TournamentRoundTeams_SeedNumber CHECK (SeedNumber >= 0),
        CONSTRAINT CK_TournamentRoundTeams_FinalRank CHECK (FinalRank >= 0),
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
    CREATE INDEX IX_TournamentRoundTeams_Tournament ON TournamentRoundTeams(TournamentId);
    CREATE INDEX IX_TournamentRoundTeams_Division ON TournamentRoundTeams(DivisionId);
    CREATE INDEX IX_TournamentRoundTeams_Round ON TournamentRoundTeams(RoundId);
    CREATE INDEX IX_TournamentRoundTeams_Team ON TournamentRoundTeams(TeamId);
    CREATE INDEX IX_TournamentRoundTeams_TournamentRound ON TournamentRoundTeams(TournamentRoundId);
    CREATE INDEX IX_TournamentRoundTeams_SeedNumber ON TournamentRoundTeams(SeedNumber);
    CREATE INDEX IX_TournamentRoundTeams_FinalRank ON TournamentRoundTeams(FinalRank);
    CREATE INDEX IX_TournamentRoundTeams_Points ON TournamentRoundTeams(Points);
    
    PRINT 'TournamentRoundTeams table created successfully.';
END
ELSE
BEGIN
    PRINT 'TournamentRoundTeams table already exists.';
END
GO

PRINT 'Tournament Rounds Management schema setup completed.';
