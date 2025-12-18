# Round to RoundTemplate Refactoring

## Overview
This document tracks the refactoring of the `Round` entity to `RoundTemplate` throughout the codebase, including:
- Renaming the entity class
- Renaming database table from `Rounds` to `RoundTemplates`
- Renaming `RoundId` fields to `RoundTemplateId`
- Updating all references in code and SQL scripts

## Status: IN PROGRESS

## Completed C# Code Changes

### ✅ Models
- [x] Created `RoundTemplate.cs` (renamed from `Round.cs`)
- [x] Updated `TournamentRound.cs` - Changed `RoundId` to `RoundTemplateId` and type to `RoundTemplate`
- [x] Updated `TournamentRoundTeam.cs` - Changed `RoundId` to `RoundTemplateId` and type to `RoundTemplate`
- [x] Updated `Match.cs` - Changed `RoundId` to `RoundTemplateId`, navigation property to `RoundTemplate`

### ✅ Data/Configuration
- [x] Updated `ApplicationDbContext.cs`:
  - Changed `DbSet<Round> Rounds` to `DbSet<RoundTemplate> RoundTemplates`
  - Updated entity configuration with `.ToTable("RoundTemplates")`
  - Updated foreign key references from `RoundId` to `RoundTemplateId`
- [x] Updated `DatabaseInitialization.cs` - Changed method signature and references to use `RoundTemplate`

### ✅ Services
- [x] Updated `IRoundService.cs` - All method signatures changed to use `RoundTemplate`
- [x] Updated `RoundService.cs` - All methods updated to use `RoundTemplate` and `_context.RoundTemplates`

## Pending C# Code Changes

### ⏳ Services - Need to Update References
The following services reference `RoundId` and need to be updated to `RoundTemplateId`:

1. **TournamentRoundService.cs**
   - Search for: `RoundId` (appears in multiple LINQ queries)
   - Update foreign key references in queries

2. **TournamentService.cs**
   - Lines referencing `m.RoundId`, `tr.RoundId`
   - Update queries that access Round data

3. **MatchService.cs**
   - Update queries with `m.RoundId` filter conditions

4. **ActiveTournamentService.cs**
   - Update references to `RoundId`

5. **ISignalRNotificationService.cs** & **SignalRNotificationService.cs**
   - Update method signatures: `NotifyRoundStartedAsync(Round round)` → `NotifyRoundStartedAsync(RoundTemplate round)`
   - Update method signatures: `NotifyRoundFinishedAsync(Round round)` → `NotifyRoundFinishedAsync(RoundTemplate round)`

6. **Hubs/TournamentHub.cs**
   - Update: `SendRoundUpdate(Round round)` → `SendRoundUpdate(RoundTemplate round)`

### ⏳ Controllers - Need to Update References
The following controllers need updates:

1. **RoundsController.cs** (Admin)
   - Update all references from `Round` to `RoundTemplate`
   - Update `_dbContext.Rounds` to `_dbContext.RoundTemplates`

2. **TournamentRoundsController.cs** (Admin)
   - Update all `RoundId` references to `RoundTemplateId`
   - Update all `_context.Rounds` queries to `_context.RoundTemplates`

3. **MatchesController.cs** (Admin)
   - Update `RoundId` parameter references to `RoundTemplateId`
   - Update `_dbContext.Rounds` queries to `_dbContext.RoundTemplates`

4. **UpdatesController.cs** (Admin)
   - Check for any `RoundId` references

### ⏳ Views - May Need Updates
Views that may need updates (check for `RoundId` in ViewData/Model):
- Admin/Matches/Create.cshtml
- Admin/Matches/Edit.cshtml
- Admin/Rounds/*.cshtml
- Admin/TournamentRounds/*.cshtml

### ⏳ Migrations
Need to update EF Core migration files:
- `ApplicationDbContextModelSnapshot.cs` - Update all references to use `RoundTemplate` and `RoundTemplates`
- Consider generating a new migration to capture all these changes

## Database Migration

### ✅ Created Migration Script
- [x] Created `database/migration-rename-rounds-to-roundtemplates.sql`
  - Drops foreign key constraints from Matches, TournamentRounds, TournamentRoundTeams
  - Renames `Rounds` table to `RoundTemplates`
  - Renames `RoundId` columns to `RoundTemplateId` in:
    * Matches table
    * TournamentRounds table
    * TournamentRoundTeams table
  - Recreates foreign key constraints

### ⏳ Update SQL Setup Scripts

1. **database/setup.sql**
   - Line 84-100: Rename table creation from `Rounds` to `RoundTemplates`
   - Line 116: Change `RoundId` to `RoundTemplateId` in Matches table
   - Line 133: Update FK name from `FK_Matches_Rounds` to `FK_Matches_RoundTemplates`
   - Line 192-201: Update INSERT statements to use `RoundTemplates` table
   - Line 207: Update index name from `IX_Matches_Round` to `IX_Matches_RoundTemplate`

2. **database/setup-complete.sql**
   - Similar updates as setup.sql
   - Lines 84-100: Table creation
   - Lines referencing Rounds table and RoundId
   - Foreign key references in TournamentRounds and TournamentRoundTeams tables
   - Lines 500-511: INSERT statements

3. **database/tournament-rounds-setup.sql**
   - Line 40: Update FK name from `FK_TournamentRounds_Round` to `FK_TournamentRounds_RoundTemplate`
   - Update reference from `Rounds(Id)` to `RoundTemplates(Id)`
   - Line 100: Update FK name from `FK_TournamentRoundTeams_Round` to `FK_TournamentRoundTeams_RoundTemplate`

4. **database/migration-schema-sync-complete.sql**
   - Lines 62-143: Update all references from `Rounds` to `RoundTemplates`
   - Lines 141-142, 249-250: Update FK references

5. **database/migration-add-round-recommendations.sql**
   - Lines 5-65: Update all `Rounds` table references to `RoundTemplates`

6. **database/cleanup.sql**
   - Line 31-32: Update from `dbo.Rounds` to `dbo.RoundTemplates`

## Testing Checklist

After all changes are made:
- [ ] Delete old `Round.cs` file manually
- [ ] Run dotnet build - ensure no compilation errors
- [ ] Run the migration script on a test database
- [ ] Verify all foreign keys are correctly established
- [ ] Test CRUD operations on rounds in Admin UI
- [ ] Test TournamentRound creation and management
- [ ] Test Match creation (should reference RoundTemplateId)
- [ ] Verify SignalR notifications still work
- [ ] Run integration tests if available

## Notes

- The underlying concept hasn't changed - we're just clarifying that these are "templates" for rounds
- All foreign key relationships remain the same, just with renamed columns
- The `Round` navigation property name can remain as-is for readability in code (only type changes to `RoundTemplate`)
- Consider generating a new EF Core migration after all C# changes are complete

## Next Steps

1. Search and replace in services for `RoundId` → `RoundTemplateId`
2. Update all controllers to use new field names
3. Update SQL scripts (setup.sql, setup-complete.sql, etc.)
4. Generate new EF Core migration
5. Test thoroughly in development environment
6. Update any API documentation
