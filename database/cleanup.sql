-- Drop all tables
USE stjago-volleyball-demo;
GO

IF OBJECT_ID('dbo.MatchUpdates', 'U') IS NOT NULL
    DROP TABLE dbo.MatchUpdates;
GO

IF OBJECT_ID('dbo.Matches', 'U') IS NOT NULL
    DROP TABLE dbo.Matches;
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

IF OBJECT_ID('dbo.Announcements', 'U') IS NOT NULL
    DROP TABLE dbo.Announcements;
GO

IF OBJECT_ID('dbo.AspNetUsers', 'U') IS NOT NULL
    DROP TABLE dbo.AspNetUsers;
GO

IF OBJECT_ID('dbo.AspNetRoles', 'U') IS NOT NULL
    DROP TABLE dbo.AspNetRoles;
GO

IF OBJECT_ID('dbo.AspNetRoleClaims', 'U') IS NOT NULL
    DROP TABLE dbo.AspNetRoleClaims;
GO

IF OBJECT_ID('dbo.AspNetUserClaims', 'U') IS NOT NULL
    DROP TABLE dbo.AspNetUserClaims;
GO

IF OBJECT_ID('dbo.AspNetUserLogins', 'U') IS NOT NULL
    DROP TABLE dbo.AspNetUserLogins;
GO

IF OBJECT_ID('dbo.AspNetUserRoles', 'U') IS NOT NULL
    DROP TABLE dbo.AspNetUserRoles;
GO

IF OBJECT_ID('dbo.AspNetUserTokens', 'U') IS NOT NULL
    DROP TABLE dbo.AspNetUserTokens;
GO