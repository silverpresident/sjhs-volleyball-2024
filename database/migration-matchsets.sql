-- Migration script to add MatchSet table and update Match table
-- Run this script against your SQL Server database

BEGIN TRANSACTION;

-- Add new columns to Match table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Matches') AND name = 'CurrentSetNumber')
BEGIN
    ALTER TABLE Matches ADD CurrentSetNumber INT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Matches') AND name = 'IsLocked')
BEGIN
    ALTER TABLE Matches ADD IsLocked BIT NOT NULL DEFAULT 0;
END

-- Create MatchSets table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MatchSets')
BEGIN
    CREATE TABLE MatchSets (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        MatchId UNIQUEIDENTIFIER NOT NULL,
        SetNumber INT NOT NULL,
        HomeTeamScore INT NOT NULL DEFAULT 0,
        AwayTeamScore INT NOT NULL DEFAULT 0,
        IsFinished BIT NOT NULL DEFAULT 0,
        IsLocked BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(MAX) NOT NULL DEFAULT 'system',
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedBy NVARCHAR(MAX) NOT NULL DEFAULT 'system',
        
        -- Foreign key constraint
        CONSTRAINT FK_MatchSets_Matches_MatchId FOREIGN KEY (MatchId) 
            REFERENCES Matches(Id) ON DELETE CASCADE,
        
        -- Unique constraint on MatchId and SetNumber combination
        CONSTRAINT UQ_MatchSets_MatchId_SetNumber UNIQUE (MatchId, SetNumber)
    );
    
    -- Create index for better query performance
    CREATE INDEX IX_MatchSets_MatchId ON MatchSets(MatchId);
END

COMMIT TRANSACTION;

PRINT 'Migration completed successfully. MatchSets table created and Match table updated.';
