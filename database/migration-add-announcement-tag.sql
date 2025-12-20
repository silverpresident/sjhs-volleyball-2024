-- Migration: Add Tag column to Announcements table
-- Date: 2025-12-20
-- Description: Adds a nullable Tag column (NVARCHAR(256)) with an index for quick lookups

-- Check if the column already exists before adding it
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Announcements]') 
    AND name = 'Tag'
)
BEGIN
    PRINT 'Adding Tag column to Announcements table...';
    
    -- Add the Tag column as nullable NVARCHAR(256)
    ALTER TABLE [dbo].[Announcements]
    ADD [Tag] NVARCHAR(256) NULL;
    
    PRINT 'Tag column added successfully.';
END
ELSE
BEGIN
    PRINT 'Tag column already exists in Announcements table.';
END
GO

-- Create an index on the Tag column for improved query performance
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE object_id = OBJECT_ID(N'[dbo].[Announcements]') 
    AND name = 'IX_Announcements_Tag'
)
BEGIN
    PRINT 'Creating index on Tag column...';
    
    CREATE NONCLUSTERED INDEX [IX_Announcements_Tag]
    ON [dbo].[Announcements] ([Tag])
    WHERE [Tag] IS NOT NULL;
    
    PRINT 'Index IX_Announcements_Tag created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_Announcements_Tag already exists.';
END
GO

PRINT 'Migration completed: Add Tag column to Announcements table';
GO
