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
    
    -- Rename TeamsAdvancing to AdvancingTeamsCount
    EXEC sp_rename 'TournamentRounds.TeamsAdvancing', 'AdvancingTeamsCount', 'COLUMN';

    -- Rename TeamSelectionMethod to AdvancingTeamSelectionStrategy
    EXEC sp_rename 'TournamentRounds.TeamSelectionMethod', 'AdvancingTeamSelectionStrategy', 'COLUMN';
    PRINT 'TournamentRounds table udpated successfully.';
END
ELSE
BEGIN
    PRINT 'TournamentRounds table already exists.';
END
GO 