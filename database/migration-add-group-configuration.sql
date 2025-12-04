-- Migration: Add Group Configuration to TournamentRounds
-- Date: 2025-12-03
-- Description: Adds TeamsPerGroup and GroupsInRound columns to TournamentRounds table
-- These fields support flexible group configuration for tournament rounds

-- Add new columns to TournamentRounds table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TournamentRounds]') AND name = 'TeamsPerGroup')
BEGIN
    ALTER TABLE [dbo].[TournamentRounds]
    ADD [TeamsPerGroup] INT NULL;
    
    PRINT 'Added TeamsPerGroup column to TournamentRounds table';
END
ELSE
BEGIN
    PRINT 'TeamsPerGroup column already exists in TournamentRounds table';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TournamentRounds]') AND name = 'GroupsInRound')
BEGIN
    ALTER TABLE [dbo].[TournamentRounds]
    ADD [GroupsInRound] INT NULL;
    
    PRINT 'Added GroupsInRound column to TournamentRounds table';
END
ELSE
BEGIN
    PRINT 'GroupsInRound column already exists in TournamentRounds table';
END
GO

-- Verify the changes
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TournamentRounds'
AND COLUMN_NAME IN ('TeamsPerGroup', 'GroupsInRound')
ORDER BY COLUMN_NAME;
GO

PRINT 'Migration completed successfully';
GO
