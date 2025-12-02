-- Migration: Add RegistrationUrl to Tournament table
-- Date: 2025-12-02
-- Description: Adds a nullable RegistrationUrl field to the Tournament entity

-- Add RegistrationUrl column to Tournaments table
ALTER TABLE Tournaments
ADD RegistrationUrl NVARCHAR(MAX) NULL;

GO

-- Optional: Add a comment for documentation
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'URL for tournament registration', 
    @level0type = N'SCHEMA', 
    @level0name = N'dbo', 
    @level1type = N'TABLE', 
    @level1name = N'Tournaments', 
    @level2type = N'COLUMN', 
    @level2name = N'RegistrationUrl';

GO
