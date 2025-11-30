-- Drop all tables
USE stjago-volleyball-demo;
GO

IF OBJECT_ID('dbo.MatchSets', 'U') IS NOT NULL
    DROP TABLE dbo.MatchSets;
GO

IF OBJECT_ID('dbo.MatchUpdates', 'U') IS NOT NULL
    DROP TABLE dbo.MatchUpdates;
GO

IF OBJECT_ID('dbo.Matches', 'U') IS NOT NULL
    DROP TABLE dbo.Matches;
GO

IF OBJECT_ID('dbo.TournamentDivisions', 'U') IS NOT NULL
    DROP TABLE dbo.TournamentDivisions;
GO

IF OBJECT_ID('dbo.TournamentTeamDivisions', 'U') IS NOT NULL
    DROP TABLE dbo.TournamentTeamDivisions;
GO

IF OBJECT_ID('dbo.Rounds', 'U') IS NOT NULL
    DROP TABLE dbo.Rounds;
GO

IF OBJECT_ID('dbo.Teams', 'U') IS NOT NULL
    DROP TABLE dbo.Teams;
GO

IF OBJECT_ID('dbo.Divisions', 'U') IS NOT NULL
    DROP TABLE dbo.Divisions;
GO

IF OBJECT_ID('dbo.Bulletins', 'U') IS NOT NULL
    DROP TABLE dbo.Bulletins;
GO

IF OBJECT_ID('dbo.Tournaments', 'U') IS NOT NULL
    DROP TABLE dbo.Tournaments;
GO


DROP TRIGGER IF EXISTS dbo.TR_Teams_UpdatedAt;
DROP TRIGGER IF EXISTS dbo.TR_Matches_UpdatedAt;
DROP TRIGGER IF EXISTS dbo.TR_MatchUpdates_UpdatedAt;
DROP TRIGGER IF EXISTS dbo.TR_Bulletins_UpdatedAt;
