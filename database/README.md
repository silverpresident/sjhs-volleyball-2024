# Database Scripts Documentation

This folder contains SQL scripts for setting up and maintaining the ST JAGO Volleyball Rally Manager database.

## Overview

The database schema includes tables for managing volleyball tournaments, including:
- **Core Tables**: Tournaments, Divisions, Teams, Rounds
- **Relationship Tables**: TournamentDivisions, TournamentTeamDivisions
- **Round Management**: TournamentRounds, TournamentRoundTeams
- **Match Management**: Matches, MatchSets, MatchUpdates
- **Communication**: Bulletins, Announcements, AnnouncementHistoryLogs, ChatRooms, ChatMessages
- **Chat**: ChatRooms, ChatMessages, ChatRoomMemberships

## Setup Scripts

### setup.sql
**Purpose**: Complete database setup script for fresh installations.

**Usage**: Run this script on a fresh SQL Server instance to create the complete database schema.

```sql
-- Execute in SQL Server Management Studio or Azure Data Studio
sqlcmd -S your-server -d master -i setup.sql
```

**What it includes**:
- Creates the `stjago-volleyball-demo` database
- Creates all core tables (Tournaments, Divisions, Teams, Rounds)
- Creates relationship tables (TournamentDivisions, TournamentTeamDivisions)
- Creates Tournament Rounds Management tables (TournamentRounds, TournamentRoundTeams)
- Creates Match Management tables (Matches, MatchSets, MatchUpdates)
- Creates Communication tables (Bulletins, Announcements, AnnouncementHistoryLogs)
- Sets up all foreign key relationships with proper CASCADE behavior
- Creates indexes for performance optimization
- Inserts initial seed data (Divisions and Rounds)

### setup-complete.sql
**Purpose**: Alternative complete setup script (identical to setup.sql).

**Usage**: Same as setup.sql - use for fresh database installations.

## Migration Scripts

### migration-schema-sync-complete.sql
**Purpose**: Comprehensive migration script to update existing databases to the latest schema.

**⚠️ NOTE**: This script is currently **outdated** and does not include the latest Chat tables or Announcement Tags. It is recommended to use `setup.sql` for fresh installations.

**Usage**: Run this on an existing database to apply all schema updates.

```sql
-- Execute on existing database
USE [stjago-volleyball-demo];
GO
-- Then run the migration script
```

**What it does**:
1. Adds `RegistrationUrl` to Tournaments table
2. Updates Rounds table with recommendation fields
3. Creates/Updates TournamentRounds table with all new fields
4. Creates/Updates TournamentRoundTeams table
5. Creates/Updates MatchSets table
6. Creates/Updates Announcements and AnnouncementHistoryLogs tables
7. Adds missing columns to existing tables
8. Renames old columns to new naming conventions
9. Verifies foreign key constraints

**Safe to run multiple times**: The script checks for existing tables/columns before making changes.

### Individual Migration Scripts

#### migration-add-registrationurl.sql
- Adds `RegistrationUrl NVARCHAR(MAX)` column to Tournaments table
- Date: 2025-12-02

#### migration-add-group-configuration.sql
- Adds `TeamsPerGroup INT` and `GroupsInRound INT` to TournamentRounds table
- Date: 2025-12-03

#### migration-add-playoff-fields.sql
- Adds `QualifyingTeamsCount`, `QualifyingTeamSelectionStrategy`, and `IsPlayoff` to TournamentRounds
- Removes obsolete constraint `UQ_TournamentRounds_TournamentDivisionRoundNumber`
- Date: 2025-12-10

#### migration-add-round-recommendations.sql
- Adds recommendation fields to Rounds table:
  - `RecommendedMatchGenerationStrategy`
  - `RecommendedTeamSelectionStrategy`  
  - `IsPlayoff`
- Renames `QualifyingTeams` to `RecommendedQualifyingTeamsCount`
- Sets intelligent defaults based on round sequence
- Date: 2025-12-11

#### migration-matchsets.sql
- Creates MatchSets table for tracking individual set scores within matches
- Includes proper foreign keys and unique constraints

## Feature-Specific Setup Scripts

### tournament-rounds-setup.sql
**Purpose**: Creates TournamentRounds and TournamentRoundTeams tables.

**Usage**: Use this if you only need to add the Tournament Rounds Management feature to an existing database.

**Note**: This is now included in the main setup.sql and migration-schema-sync-complete.sql scripts.

### tournament-rounds-update.sql
**Purpose**: Renames columns in TournamentRounds table:
- `TeamsAdvancing` → `AdvancingTeamsCount`
- `TeamSelectionMethod` → `AdvancingTeamSelectionStrategy`

**Usage**: Run this if you have an old TournamentRounds table with the legacy column names.

### announcements-setup.sql
**Purpose**: Creates Announcements and AnnouncementHistoryLogs tables.

**Usage**: Use this if you only need to add the Announcements feature to an existing database.

**Note**: This is now included in the main setup.sql and migration-schema-sync-complete.sql scripts.

## Cleanup Scripts

### cleanup.sql
**Purpose**: Drops the entire database.

**⚠️ WARNING**: This will delete ALL data permanently!

**Usage**: Only use in development environments when you need to start fresh.

```sql
-- Use with extreme caution
sqlcmd -S your-server -d master -i cleanup.sql
```

### cleanup-usertables.sql
**Purpose**: Drops all tables while preserving the database.

**⚠️ WARNING**: This will delete ALL tables and data!

**Usage**: Use when you need to recreate tables but keep the database.

## Schema Overview

### Core Entity Relationships

```
Tournament (1) ----< (M) TournamentDivision >---- (M) Division (1)
                         |
                         |
                    TournamentTeamDivision
                         |
                    /----|----\
                   /            \
Team (1) ---------+              +--------- (1) Division
                                 |
                            TournamentRounds (1)
                                 |
                                 +----< (M) TournamentRoundTeams
                                 |
                                 +----< (M) Matches
                                             |
                                             +----< (M) MatchSets
                                             |
                                             +----< (M) MatchUpdates
```

### Key Tables Description

#### Tournaments
- Stores tournament information
- Fields: Name, Description, TournamentDate, IsActive, RegistrationUrl

#### TournamentRounds
- Manages rounds within tournament divisions
- Tracks round configuration, strategies, and progression
- Fields include: MatchGenerationStrategy, GroupingStrategy, AdvancingTeamsCount, QualifyingTeamsCount, IsPlayoff

#### TournamentRoundTeams
- Tracks team participation in specific tournament rounds
- Stores round-specific statistics and rankings
- Fields: SeedNumber, Rank, Points, Wins, Losses, Sets, Scores

#### Matches
- Stores match information
- Linked to Tournament, Division, Round, and Teams
- Fields: MatchNumber, ScheduledTime, CourtLocation, Scores, IsFinished

#### MatchSets
- Tracks individual set scores within a match
- Unique constraint on (MatchId, SetNumber)
- Fields: SetNumber, HomeTeamScore, AwayTeamScore, IsFinished

#### Announcements
- Tournament-specific announcements
- Supports repeat announcements and priority levels
- Linked to AnnouncementHistoryLogs for tracking announcement history
- Includes `Tag` field for categorization

#### Chat Tables
- **ChatRooms**: Manages chat rooms (Public, Private, etc.)
- **ChatMessages**: Stores messages with markdown support
- **ChatRoomMemberships**: Tracks user membership in rooms

## Best Practices

### For Fresh Installations
1. Run `setup.sql` on a fresh SQL Server instance
2. Verify all tables were created successfully
3. Check that seed data (Divisions and Rounds) was inserted

### For Existing Databases
1. **Backup your database first!**
2. Run `migration-schema-sync-complete.sql` to apply all updates
3. Review the output messages to confirm successful execution
4. Test the application thoroughly

### For Development
1. Use `cleanup.sql` to reset database (development only!)
2. Run `setup.sql` to recreate schema
3. Run your application's data seeding if needed

### For Production
1. **Always backup before any schema changes!**
2. Review migration scripts carefully
3. Test migrations on a staging database first
4. Run migrations during maintenance windows
5. Monitor application logs after deployment

## Entity Model Synchronization

The SQL scripts are kept in sync with the C# entity models located at:
- `src/VolleyballRallyManager.Lib/Models/`

All tables include standard audit fields from [`BaseEntity`](../src/VolleyballRallyManager.Lib/Models/BaseEntity.cs):
- `Id` (UNIQUEIDENTIFIER)
- `CreatedAt` (DATETIME2)
- `CreatedBy` (NVARCHAR(256))
- `UpdatedAt` (DATETIME2)
- `UpdatedBy` (NVARCHAR(256))

## Default Values

### Date Functions
- All scripts use `GETDATE()` instead of `GETUTCDATE()` per project conventions
- Timestamps are stored in DATETIME2 format

### User Tracking
- Default value for CreatedBy and UpdatedBy is `'system'`
- These should be overridden by the application with actual user information

## Foreign Key Cascade Behaviors

- **CASCADE DELETE**: Tournament → TournamentDivisions, Matches, TournamentRounds, Announcements
- **NO ACTION**: Most other relationships to prevent accidental data loss

## Index Strategy

Indexes are created on:
- All foreign key columns
- Frequently queried fields (TournamentId, DivisionId, RoundId, IsHidden, etc.)
- Unique constraints (TournamentDivision combinations, MatchSet combinations, etc.)

## Troubleshooting

### "Object already exists" errors
These are expected and safe - the scripts check for existing objects before creating them.

### Foreign key constraint errors
Ensure you're running scripts in the correct order. The main setup.sql creates all dependencies in the right sequence.

### Permission errors
Ensure your SQL user has `db_owner` or appropriate CREATE/ALTER permissions.

## Version History

- **2025-12-20**: Added Chat tables and Announcement Tags. Removed "Round 3" and renamed "Semi-finals" in seed data.
- **2025-12-15**: Consolidated all migrations into migration-schema-sync-complete.sql
- **2025-12-11**: Added round recommendation fields
- **2025-12-10**: Added playoff fields to tournament rounds
- **2025-12-03**: Added group configuration fields
- **2025-12-02**: Added registration URL to tournaments
- **Initial**: Created base schema with core tables

## Related Documentation

- [Authentication Setup](../docs/AUTHENTICATION_SETUP.md)
- [Tournament Rounds Implementation](../docs/TOURNAMENT_ROUNDS_IMPLEMENTATION.md)
- [Announcer Feature](../docs/ANNOUNCER_FEATURE.md)
- [Auto-Seed Feature](../docs/AUTOSEED_FEATURE.md)
- [User Management Guide](../docs/USER_MANAGEMENT_GUIDE.md)
