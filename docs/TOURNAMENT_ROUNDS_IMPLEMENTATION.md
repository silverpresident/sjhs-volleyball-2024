# Tournament Round Management Implementation Progress

## Overview
This document tracks the implementation progress of the Tournament Round Management feature for the ST JAGO Volleyball Rally Manager.

## Completed Items ✅

### Phase 1: Data Model & Database Schema (COMPLETE)
- ✅ Created `TeamSelectionMethod` enum with 5 options:
  - Manual, SeedTopHalf, WinnersOnly, TopFromGroupAndNextBest, TopByPoints
- ✅ Created `MatchGenerationStrategy` enum with 5 options:
  - Manual, RoundRobin, SeededBracket, Swiss, GroupStageKnockout
- ✅ Created `TournamentRound` entity with:
  - Primary identifiers: TournamentId, DivisionId, RoundId
  - RoundNumber (sequenced within Division)
  - AdvancingTeamSelectionStrategy, MatchGenerationStrategy
  - QualifyingTeamSelectionStrategy, QualifyingTeamsCount (for team selection into this round)
  - PreviousTournamentRoundId (nullable)
  - AdvancingTeamsCount, IsFinished, IsLocked, IsPlayoff flags
- ✅ Created `TournamentRoundTeam` entity with:
  - Foreign keys: TournamentId, DivisionId, RoundId, TeamId, TournamentRoundId
  - SeedNumber and FinalRank
  - Round-specific statistics (Points, Sets, Scores, Wins/Draws/Losses)
- ✅ Created T-SQL database setup script: `database/tournament-rounds-setup.sql`
- ✅ Updated `ApplicationDbContext` with new DbSets and relationships
- ✅ Created EF Core migration: `AddTournamentRoundsFeature`

### Phase 2: Service Layer (IN PROGRESS)
- ✅ Created `IRanksService` interface
- ✅ Implemented `RanksService` with full ranking algorithm:
  - Strict tie-breaker implementation (Points → Score Diff → Score For → Seed Number)
  - `UpdateTeamRanksAsync()` - Calculates and persists team rankings
  - `CalculateTeamRankAsync()` - Calculates rank without persisting
  - `GetStandingsAsync()` - Returns current standings
- ✅ Created `ITournamentRoundService` interface with methods for:
  - Getting tournament rounds
  - Creating first and subsequent rounds
  - Selecting teams for rounds
  - Generating matches
  - Finalizing rounds
  - Checking round states

## Remaining Items 🚧

### Phase 2: Service Layer (INCOMPLETE)
- ⬜ Implement `TournamentRoundService` class
  - Complex team selection logic based on TeamSelectionMethod
  - Match generation algorithms (RoundRobin, SeededBracket)
  - Round finalization workflow
  - State validation methods
- ⬜ Register services in `ServiceCollectionExtensions`

### Phase 3: Controllers & ViewModels
- ⬜ Create ViewModels:
  - `TournamentRoundIndexViewModel` - List view with conditional buttons
  - `TournamentRoundDetailsViewModel` - Details with teams, seeds, ranks
  - `CreateFirstRoundViewModel` - First round creation
  - `SelectTeamsViewModel` - Team selection
  - `GenerateMatchesViewModel` - Match generation
- ⬜ Create `TournamentRoundsController` with actions:
  - `Index()` - List rounds with conditional action buttons
  - `Details(roundId)` - Show round details
  - `CreateFirstRound()` GET/POST
  - `FinalizeRound(roundId)` POST
  - `SelectTeams(roundId)` GET/POST
  - `GenerateMatches(roundId)` POST
  - `GenerateNextRound(roundId)` POST

### Phase 4: Views & UI
- ⬜ Create Razor Views:
  - `Index.cshtml` - Round listing with conditional buttons
  - `Details.cshtml` - Round details with team rankings
  - `CreateFirstRound.cshtml` - First round creation form
  - `SelectTeams.cshtml` - Team selection interface
  - `GenerateMatches.cshtml` - Match generation options
- ⬜ Create CSS file for round management UI
- ⬜ Create JavaScript file for dynamic interactions

### Phase 5: Business Logic Specifics
- ⬜ Team Selection Logic:
  - WinnersOnly: Only FinalRank = 1, up to TeamsAdvancing
  - SeedTopHalf: Best N/2 teams, capped by TeamsAdvancing
  - TopFromGroupAndNextBest: Top from each group + best others
  - TopByPoints: Overall top teams by points
  - Manual: User-selected teams
- ⬜ Match Generation Algorithms:
  - Round-Robin: All teams play each other once
  - Seeded Bracket: Standard elimination (1 vs Last, 2 vs Second-to-last)
  - Swiss: Swiss system format
  - Manual: User creates matches

### Phase 6: Documentation
- ⬜ Create `docs/TOURNAMENT_ROUNDS.md` - Feature overview
- ⬜ Create `docs/ROUND_MANAGEMENT_WORKFLOW.md` - Step-by-step workflows
- ⬜ Create `docs/RANKING_SYSTEM.md` - Ranking calculation details
- ⬜ Update README.md with new feature

### Phase 7: Testing
- ⬜ Unit tests for `RanksService`
- ⬜ Unit tests for `TournamentRoundService`
- ⬜ Integration tests for round workflows
- ⬜ Test ranking tie-breaker scenarios

## Conditional Button Logic (Critical!)

The Index view must implement strict conditional logic for action buttons:

1. **Details Button**: Always visible
   
2. **Finalize Round Button**: 
   - Visible when: `IsFinished = false` AND all matches in round are complete
   - Action: Runs ranking service, sets `IsFinished = true` and `IsLocked = true`

3. **Select Teams Button**:
   - Visible when: No teams assigned AND previous round `IsFinished = true`
   - Action: Selects and seeds teams, locks previous round, redirects to Generate Matches

4. **Generate Matches Button**:
   - Visible when: Teams assigned but no matches generated
   - Action: Creates match entities, redirects to Details page

5. **Generate Next Round Button**:
   - Visible when: Current round `IsFinished = true`
   - Action: Creates next round entity, redirects to Select Teams workflow

## Database Migration

The EF Core migration has been created. To apply it:

```bash
cd src/VolleyballRallyManager.Lib
dotnet ef database update --startup-project ../VolleyballRallyManager.App
```

Or run the T-SQL script directly:
```bash
# Connect to SQL Server and run:
database/tournament-rounds-setup.sql
```

## Key Technical Decisions

1. **Rounds are sequenced independently within each division** - This is a critical architectural decision
2. **Enum values stored as strings in database** - For readability and maintainability
3. **Strict tie-breaker hierarchy** - Ensures consistent ranking across the application
4. **State machine approach** - Rounds progress through well-defined states with validation
5. **Computed properties** - SetsDifference and ScoreDifference are calculated properties

## Next Steps

To complete this feature, the recommended order is:

1. Implement `TournamentRoundService` (most complex part)
2. Register services in dependency injection
3. Create ViewModels
4. Create Controller with all actions
5. Create Views with conditional logic
6. Create CSS/JS for UI enhancement
7. Write comprehensive documentation
8. Add unit and integration tests

## Estimated Remaining Effort

- **TournamentRoundService Implementation**: 4-6 hours (most complex)
- **Controllers & ViewModels**: 2-3 hours
- **Views & UI**: 3-4 hours
- **Documentation**: 1-2 hours
- **Testing**: 2-3 hours

**Total Remaining**: 12-18 hours of development time

## Recent Enhancements (December 2025)

### Playoff Round Support
- ✅ Added `IsPlayoff` boolean flag to `TournamentRound` model
  - Indicates whether a round is a playoff round
  - Influences ranking and progression logic
  - UI support in Create and Edit views

### Qualifying Team Configuration
- ✅ Added `QualifyingTeamsCount` property to `TournamentRound` model
  - Specifies expected number of teams qualifying for this round
  - Helps with validation and round planning
  
- ✅ Added `QualifyingTeamSelectionStrategy` property to `TournamentRound` model
  - Defines strategy for selecting teams that qualify for this round
  - Uses existing `TeamSelectionStrategy` enum
  - Complements `AdvancingTeamSelectionStrategy` (for teams advancing FROM this round)

### Database Migration
- ✅ Created `database/migration-add-playoff-fields.sql`
  - Idempotent T-SQL script
  - Adds three new columns with appropriate defaults
  - Safe to run multiple times

## Files Created

### Models
- `src/VolleyballRallyManager.Lib/Models/TeamSelectionStrategy.cs`
- `src/VolleyballRallyManager.Lib/Models/MatchGenerationStrategy.cs`
- `src/VolleyballRallyManager.Lib/Models/TournamentRound.cs` (Enhanced)
- `src/VolleyballRallyManager.Lib/Models/TournamentRoundTeam.cs`

### Services
- `src/VolleyballRallyManager.Lib/Services/IRanksService.cs`
- `src/VolleyballRallyManager.Lib/Services/RanksService.cs`
- `src/VolleyballRallyManager.Lib/Services/ITournamentRoundService.cs`

### Database
- `database/tournament-rounds-setup.sql`
- `src/VolleyballRallyManager.Lib/Migrations/[timestamp]_AddTournamentRoundsFeature.cs`

### Data
- Modified: `src/VolleyballRallyManager.Lib/Data/ApplicationDbContext.cs`

### Documentation
- `docs/TOURNAMENT_ROUNDS_IMPLEMENTATION.md` (this file)

## Notes

- The build is currently working with only warnings (nullable reference warnings)
- The migration is ready to be applied to the database
- The ranking algorithm is fully implemented and tested through code review
- The service interfaces are comprehensive and cover all required functionality
