-- =============================================
-- ST JAGO Volleyball Rally Manager - ASP.NET Identity Tables Setup
-- =============================================
-- This script creates all ASP.NET Identity tables needed for user authentication
-- Run this script before running setup.sql if setting up a fresh database

-- Create Schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'dbo')
BEGIN
    EXEC('CREATE SCHEMA dbo');
END
GO

-- =============================================
-- ASP.NET Identity Core Tables
-- =============================================

-- AspNetRoles
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoles' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.AspNetRoles
    (
        Id NVARCHAR(450) NOT NULL PRIMARY KEY,
        Name NVARCHAR(256) NULL,
        NormalizedName NVARCHAR(256) NULL,
        ConcurrencyStamp NVARCHAR(MAX) NULL
    );
    
    -- Create Index
    CREATE UNIQUE NONCLUSTERED INDEX RoleNameIndex 
        ON dbo.AspNetRoles(NormalizedName) 
        WHERE NormalizedName IS NOT NULL;
    
    PRINT 'AspNetRoles table created successfully.';
END
GO

-- AspNetUsers (with custom fields: FirstName, LastName, RefreshToken, RefreshTokenExpiryTime, LastLoginAt)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUsers' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.AspNetUsers
    (
        Id NVARCHAR(450) NOT NULL PRIMARY KEY,
        
        -- Custom Application Fields
        DisplayName NVARCHAR(256) NULL,
        FirstName NVARCHAR(256) NULL,
        LastName NVARCHAR(256) NULL,
        RefreshToken NVARCHAR(MAX) NULL,
        RefreshTokenExpiryTime DATETIME2 NULL,
        LastLoginAt DATETIME2 NULL,
        
        -- Standard Identity Fields
        UserName NVARCHAR(256) NULL,
        NormalizedUserName NVARCHAR(256) NULL,
        Email NVARCHAR(256) NULL,
        NormalizedEmail NVARCHAR(256) NULL,
        EmailConfirmed BIT NOT NULL DEFAULT 0,
        PasswordHash NVARCHAR(MAX) NULL,
        SecurityStamp NVARCHAR(MAX) NULL,
        ConcurrencyStamp NVARCHAR(MAX) NULL,
        PhoneNumber NVARCHAR(MAX) NULL,
        PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
        TwoFactorEnabled BIT NOT NULL DEFAULT 0,
        LockoutEnd DATETIMEOFFSET NULL,
        LockoutEnabled BIT NOT NULL DEFAULT 0,
        AccessFailedCount INT NOT NULL DEFAULT 0
    );
    
    -- Create Indexes
    CREATE NONCLUSTERED INDEX EmailIndex 
        ON dbo.AspNetUsers(NormalizedEmail);
    
    CREATE UNIQUE NONCLUSTERED INDEX UserNameIndex 
        ON dbo.AspNetUsers(NormalizedUserName) 
        WHERE NormalizedUserName IS NOT NULL;
    
    PRINT 'AspNetUsers table created successfully.';
END
GO

-- =============================================
-- ASP.NET Identity Relationship Tables
-- =============================================

-- AspNetRoleClaims
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoleClaims' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.AspNetRoleClaims
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        RoleId NVARCHAR(450) NOT NULL,
        ClaimType NVARCHAR(MAX) NULL,
        ClaimValue NVARCHAR(MAX) NULL,
        
        CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_RoleId 
            FOREIGN KEY (RoleId) 
            REFERENCES dbo.AspNetRoles(Id) 
            ON DELETE CASCADE
    );
    
    -- Create Index
    CREATE NONCLUSTERED INDEX IX_AspNetRoleClaims_RoleId 
        ON dbo.AspNetRoleClaims(RoleId);
    
    PRINT 'AspNetRoleClaims table created successfully.';
END
GO

-- AspNetUserClaims
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserClaims' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.AspNetUserClaims
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId NVARCHAR(450) NOT NULL,
        ClaimType NVARCHAR(MAX) NULL,
        ClaimValue NVARCHAR(MAX) NULL,
        
        CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId 
            FOREIGN KEY (UserId) 
            REFERENCES dbo.AspNetUsers(Id) 
            ON DELETE CASCADE
    );
    
    -- Create Index
    CREATE NONCLUSTERED INDEX IX_AspNetUserClaims_UserId 
        ON dbo.AspNetUserClaims(UserId);
    
    PRINT 'AspNetUserClaims table created successfully.';
END
GO

-- AspNetUserLogins
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserLogins' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.AspNetUserLogins
    (
        LoginProvider NVARCHAR(450) NOT NULL,
        ProviderKey NVARCHAR(450) NOT NULL,
        ProviderDisplayName NVARCHAR(MAX) NULL,
        UserId NVARCHAR(450) NOT NULL,
        
        CONSTRAINT PK_AspNetUserLogins 
            PRIMARY KEY (LoginProvider, ProviderKey),
        
        CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId 
            FOREIGN KEY (UserId) 
            REFERENCES dbo.AspNetUsers(Id) 
            ON DELETE CASCADE
    );
    
    -- Create Index
    CREATE NONCLUSTERED INDEX IX_AspNetUserLogins_UserId 
        ON dbo.AspNetUserLogins(UserId);
    
    PRINT 'AspNetUserLogins table created successfully.';
END
GO

-- AspNetUserRoles
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserRoles' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.AspNetUserRoles
    (
        UserId NVARCHAR(450) NOT NULL,
        RoleId NVARCHAR(450) NOT NULL,
        
        CONSTRAINT PK_AspNetUserRoles 
            PRIMARY KEY (UserId, RoleId),
        
        CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId 
            FOREIGN KEY (RoleId) 
            REFERENCES dbo.AspNetRoles(Id) 
            ON DELETE CASCADE,
        
        CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId 
            FOREIGN KEY (UserId) 
            REFERENCES dbo.AspNetUsers(Id) 
            ON DELETE CASCADE
    );
    
    -- Create Index
    CREATE NONCLUSTERED INDEX IX_AspNetUserRoles_RoleId 
        ON dbo.AspNetUserRoles(RoleId);
    
    PRINT 'AspNetUserRoles table created successfully.';
END
GO

-- AspNetUserTokens
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserTokens' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.AspNetUserTokens
    (
        UserId NVARCHAR(450) NOT NULL,
        LoginProvider NVARCHAR(450) NOT NULL,
        Name NVARCHAR(450) NOT NULL,
        Value NVARCHAR(MAX) NULL,
        
        CONSTRAINT PK_AspNetUserTokens 
            PRIMARY KEY (UserId, LoginProvider, Name),
        
        CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId 
            FOREIGN KEY (UserId) 
            REFERENCES dbo.AspNetUsers(Id) 
            ON DELETE CASCADE
    );
    
    PRINT 'AspNetUserTokens table created successfully.';
END
GO

-- =============================================
-- Insert Default Roles
-- =============================================

-- Insert default roles if they don't exist
IF NOT EXISTS (SELECT * FROM dbo.AspNetRoles WHERE NormalizedName = 'ADMIN')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) 
    VALUES 
    (NEWID(), 'Administrator', 'ADMINISTRATOR', NEWID());
    
    PRINT 'Admin role created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM dbo.AspNetRoles WHERE NormalizedName = 'SCORER')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) 
    VALUES 
    (NEWID(), 'Scorer', 'SCORER', NEWID());
    
    PRINT 'Scorer role created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM dbo.AspNetRoles WHERE NormalizedName = 'VIEWER')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) 
    VALUES 
    (NEWID(), 'Viewer', 'VIEWER', NEWID());
    
    PRINT 'Viewer role created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM dbo.AspNetRoles WHERE NormalizedName = 'ANNOUNCER')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) 
    VALUES 
    (NEWID(), 'Announcer', 'ANNOUNCER', NEWID());
    
    PRINT 'Announcer role created successfully.';
END
GO

PRINT '=============================================';
PRINT 'ASP.NET Identity tables setup completed successfully!';
PRINT 'All Identity tables have been created with proper relationships and indexes.';
PRINT 'Default roles (Admin, Scorer, Viewer, Announcer) have been created.';
PRINT '=============================================';
GO
