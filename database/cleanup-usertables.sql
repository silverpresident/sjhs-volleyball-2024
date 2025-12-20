-- Drop all user tables
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