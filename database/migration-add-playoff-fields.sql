-- =============================================
-- Migration: Add Playoff and Qualifying Fields to TournamentRound
-- Description: Adds QualifyingTeamsCount, QualifyingTeamSelectionStrategy, and IsPlayoff columns
-- Date: 2025-12-10
-- =============================================

-- Add QualifyingTeamsCount column if it doesn't exist
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('TournamentRounds') 
    AND name = 'QualifyingTeamsCount'
)
BEGIN
    ALTER TABLE TournamentRounds
    ADD QualifyingTeamsCount INT NOT NULL DEFAULT 0;
    
    PRINT 'Added QualifyingTeamsCount column to TournamentRounds table';
END
ELSE
BEGIN
    PRINT 'QualifyingTeamsCount column already exists in TournamentRounds table';
END
GO

-- Add QualifyingTeamSelectionStrategy column if it doesn't exist
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('TournamentRounds') 
    AND name = 'QualifyingTeamSelectionStrategy'
)
BEGIN
    ALTER TABLE TournamentRounds
    ADD QualifyingTeamSelectionStrategy NVARCHAR(50) NOT NULL DEFAULT 'Manual';
    
    PRINT 'Added QualifyingTeamSelectionStrategy column to TournamentRounds table';
END
ELSE
BEGIN
    PRINT 'QualifyingTeamSelectionStrategy column already exists in TournamentRounds table';
END
GO

-- Add IsPlayoff column if it doesn't exist
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('TournamentRounds') 
    AND name = 'IsPlayoff'
)
BEGIN
    ALTER TABLE TournamentRounds
    ADD IsPlayoff BIT NOT NULL DEFAULT 0;
    
    PRINT 'Added IsPlayoff column to TournamentRounds table';
END
ELSE
BEGIN
    PRINT 'IsPlayoff column already exists in TournamentRounds table';
END
GO


ALTER TABLE TournamentRounds
 DROP CONSTRAINT UQ_TournamentRounds_TournamentDivisionRoundNumber;
 
PRINT 'Migration completed successfully';
GO