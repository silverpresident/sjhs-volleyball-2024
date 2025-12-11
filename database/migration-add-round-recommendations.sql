-- Migration: Add recommendation properties to Rounds table
-- Description: Adds recommendation fields to help guide tournament round setup
-- Date: 2025-12-11

-- Add new recommendation columns to Rounds table
ALTER TABLE Rounds
ADD RecommendedMatchGenerationStrategy NVARCHAR(50) NOT NULL DEFAULT 'RoundRobin',
    RecommendedTeamSelectionStrategy NVARCHAR(50) NOT NULL DEFAULT 'TopByPoints',
    IsPlayoff BIT NOT NULL DEFAULT 0;

-- Rename QualifyingTeams to RecommendedQualifyingTeamsCount for clarity
EXEC sp_rename 'Rounds.QualifyingTeams', 'RecommendedQualifyingTeamsCount', 'COLUMN';

-- Set intelligent defaults for existing rounds based on their sequence

-- Preliminary/First Round (Sequence 1): Round Robin, all teams
UPDATE Rounds
SET RecommendedMatchGenerationStrategy = 'RoundRobin',
    RecommendedTeamSelectionStrategy = 'TopByPoints',
    IsPlayoff = 0
WHERE Sequence = 1;

-- Seeded Round (Sequence 2): Group Stage, top teams advance
UPDATE Rounds
SET RecommendedMatchGenerationStrategy = 'GroupStageKnockout',
    RecommendedTeamSelectionStrategy = 'TopByPoints',
    IsPlayoff = 0
WHERE Sequence = 2;

-- Round 3: Typically another seeded round
UPDATE Rounds
SET RecommendedMatchGenerationStrategy = 'GroupStageKnockout',
    RecommendedTeamSelectionStrategy = 'TopByPoints',
    IsPlayoff = 0
WHERE Sequence = 3;

-- Quarter Finals and beyond (Sequence >= 4): Playoff rounds with brackets
UPDATE Rounds
SET RecommendedMatchGenerationStrategy = 'SeededBracket',
    RecommendedTeamSelectionStrategy = 'WinnersOnly',
    IsPlayoff = 1
WHERE Sequence >= 4;

-- Verify the migration
SELECT 
    Name,
    Sequence,
    RecommendedQualifyingTeamsCount,
    RecommendedMatchGenerationStrategy,
    RecommendedTeamSelectionStrategy,
    IsPlayoff
FROM Rounds
ORDER BY Sequence;