-- =============================================
-- Migration: Rename Rounds to RoundTemplates
-- Description: Renames the Rounds table and related columns to RoundTemplates
-- Date: 2025-12-18
-- =============================================

-- Start transaction
BEGIN TRANSACTION;

PRINT 'Starting migration to rename Rounds table to RoundTemplates...';

-- =============================================
-- Step 1: Drop Foreign Key Constraints
-- =============================================
PRINT 'Step 1: Dropping foreign key constraints...';

-- Drop FK from Matches table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Matches_Rounds')
BEGIN
    ALTER TABLE [dbo].[Matches] DROP CONSTRAINT [FK_Matches_Rounds];
    PRINT '  - Dropped FK_Matches_Rounds';
END

-- Drop FK from TournamentRounds table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TournamentRounds_Round')
BEGIN
    ALTER TABLE [dbo].[TournamentRounds] DROP CONSTRAINT [FK_TournamentRounds_Round];
    PRINT '  - Dropped FK_TournamentRounds_Round';
END

-- Drop FK from TournamentRoundTeams table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TournamentRoundTeams_Round')
BEGIN
    ALTER TABLE [dbo].[TournamentRoundTeams] DROP CONSTRAINT [FK_TournamentRoundTeams_Round];
    PRINT '  - Dropped FK_TournamentRoundTeams_Round';
END

-- =============================================
-- Step 2: Rename the Rounds table to RoundTemplates
-- =============================================
PRINT 'Step 2: Renaming Rounds table to RoundTemplates...';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Rounds' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    EXEC sp_rename 'dbo.Rounds', 'RoundTemplates';
    PRINT '  - Renamed Rounds table to RoundTemplates';
END

-- =============================================
-- Step 3: Rename RoundId columns to RoundTemplateId
-- =============================================
PRINT 'Step 3: Renaming RoundId columns to RoundTemplateId...';

-- Rename in Matches table
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Matches]') AND name = 'RoundId')
BEGIN
    EXEC sp_rename 'dbo.Matches.RoundId', 'RoundTemplateId', 'COLUMN';
    PRINT '  - Renamed Matches.RoundId to RoundTemplateId';
END

-- Rename in TournamentRounds table
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TournamentRounds]') AND name = 'RoundId')
BEGIN
    EXEC sp_rename 'dbo.TournamentRounds.RoundId', 'RoundTemplateId', 'COLUMN';
    PRINT '  - Renamed TournamentRounds.RoundId to RoundTemplateId';
END

-- Rename in TournamentRoundTeams table
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TournamentRoundTeams]') AND name = 'RoundId')
BEGIN
    EXEC sp_rename 'dbo.TournamentRoundTeams.RoundId', 'RoundTemplateId', 'COLUMN';
    PRINT '  - Renamed TournamentRoundTeams.RoundId to RoundTemplateId';
END

-- =============================================
-- Step 4: Recreate Foreign Key Constraints
-- =============================================
PRINT 'Step 4: Recreating foreign key constraints...';

-- Recreate FK for Matches table
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Matches_RoundTemplates')
BEGIN
    ALTER TABLE [dbo].[Matches]
        ADD CONSTRAINT [FK_Matches_RoundTemplates]
        FOREIGN KEY ([RoundTemplateId])
        REFERENCES [dbo].[RoundTemplates]([Id])
        ON DELETE NO ACTION;
    PRINT '  - Created FK_Matches_RoundTemplates';
END

-- Recreate FK for TournamentRounds table
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TournamentRounds_RoundTemplate')
BEGIN
    ALTER TABLE [dbo].[TournamentRounds]
        ADD CONSTRAINT [FK_TournamentRounds_RoundTemplate]
        FOREIGN KEY ([RoundTemplateId])
        REFERENCES [dbo].[RoundTemplates]([Id])
        ON DELETE NO ACTION;
    PRINT '  - Created FK_TournamentRounds_RoundTemplate';
END

-- Recreate FK for TournamentRoundTeams table
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TournamentRoundTeams_RoundTemplate')
BEGIN
    ALTER TABLE [dbo].[TournamentRoundTeams]
        ADD CONSTRAINT [FK_TournamentRoundTeams_RoundTemplate]
        FOREIGN KEY ([RoundTemplateId])
        REFERENCES [dbo].[RoundTemplates]([Id])
        ON DELETE NO ACTION;
    PRINT '  - Created FK_TournamentRoundTeams_RoundTemplate';
END

-- =============================================
-- Step 5: Verify the changes
-- =============================================
PRINT 'Step 5: Verifying changes...';

-- Check if RoundTemplates table exists
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'RoundTemplates' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT '  - RoundTemplates table exists';
END
ELSE
BEGIN
    PRINT '  - ERROR: RoundTemplates table does not exist!';
    ROLLBACK TRANSACTION;
    RETURN;
END

-- Check if old Rounds table still exists
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Rounds' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT '  - WARNING: Old Rounds table still exists!';
END

-- Commit transaction
COMMIT TRANSACTION;

PRINT 'Migration completed successfully!';
PRINT '';
PRINT 'Summary:';
PRINT '  - Rounds table renamed to RoundTemplates';
PRINT '  - RoundId columns renamed to RoundTemplateId in:';
PRINT '    * Matches';
PRINT '    * TournamentRounds';
PRINT '    * TournamentRoundTeams';
PRINT '  - All foreign key constraints updated';

GO
