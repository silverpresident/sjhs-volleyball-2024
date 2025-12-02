# AutoSeed Feature Documentation

## Overview
The AutoSeed feature provides an automated way to assign seed numbers to teams within divisions in the tournament. This feature is available in the Admin area under Tournament Teams.

## Accessing AutoSeed
1. Navigate to **Admin > Tournament Teams**
2. Click the **"Auto Seed Teams"** button (yellow button with shuffle icon)

## Feature Options

### 1. Division Scope
Select which divisions to apply seeding:
- **All Divisions**: Apply seeding logic to all divisions in the active tournament
- **Specific Division**: Choose a single division from the dropdown

### 2. Seeding Method
Defines which teams are affected by the seeding process:

#### Seed Unseeded Teams (Default)
- Only assigns seeds to teams with `SeedNumber = 0` or `NULL`
- Preserves existing seed numbers
- Useful for adding new teams to an existing seeded division

#### Reseed All Teams
- Replaces **all** existing seed numbers with new sequential seeds
- All teams in the division(s) will be re-seeded from scratch
- Use when you need to completely reorganize the seeding

### 3. Sorting Method
Defines the order in which teams are processed for seeding:

#### By Creation Date (Default)
- Teams are sorted by their `CreatedAt` timestamp (oldest first)
- Provides a "first-come, first-served" approach
- Ensures teams registered earlier get lower seed numbers

#### By Team Name
- Teams are sorted alphabetically by team name
- Provides consistent, predictable ordering
- Good for neutral seeding without bias

#### Randomly
- Teams are sorted in random order
- Ensures fair, unbiased seeding
- Each execution will produce different results

### 4. Seed Placement
Determines where new seeds are placed (only applies to "Seed Unseeded Teams"):

#### Fill Gaps (Default)
- Attempts to use the lowest available seed numbers
- If seeds 1, 3, 5 exist, new teams will be assigned 2, 4, 6, etc.
- Helps maintain compact seed numbering

#### At the End
- Starts assigning seeds after the highest existing seed number
- If the max seed is 5, new teams start at 6, 7, 8, etc.
- Preserves the existing seed order completely

### 5. Seed Gap Closure
Post-seeding logic for ensuring final seed numbers are contiguous:

#### Let Gaps Remain (Default)
- Preserves any gaps in the final seed numbers
- Final seeds might be: 1, 3, 5, 7, 9
- Maintains the original seed assignments

#### Close All Gaps
- Re-sequences all seeded teams after seeding
- Ensures seeds are contiguous: 1, 2, 3, 4, 5, ...
- Eliminates duplicates and gaps
- Uses the selected Sorting Method as a tiebreaker for teams with the same seed

## Processing Logic

### Method A: Reseed All Teams
1. Selects all active teams in the division(s)
2. Sorts teams using the selected Sorting Method
3. Assigns sequential seed numbers starting from 1
4. Applies Seed Gap Closure if selected

### Method B: Seed Unseeded Teams
1. Selects teams where `SeedNumber ≤ 0`
2. Sorts unseeded teams using the selected Sorting Method
3. Determines seed placement strategy:
   - **Fill Gaps**: Finds lowest available seed numbers
   - **At the End**: Starts after max existing seed
4. Assigns seeds to unseeded teams
5. Applies Seed Gap Closure if selected

### Seed Gap Closure Process
When "Close All Gaps" is selected:
1. Retrieves all seeded teams (`SeedNumber > 0`)
2. Sorts primarily by current `SeedNumber` (ascending)
3. Applies selected Sorting Method as tiebreaker for duplicate seeds
4. Re-assigns sequential seeds: 1, 2, 3, ..., N

## Database Transaction
All seeding operations are performed within a database transaction to ensure data integrity. If any error occurs, all changes are rolled back.

## Security
- The POST action is protected with `[ValidateAntiForgeryToken]` to prevent CSRF attacks
- Only authenticated users with access to the Admin area can use this feature

## Common Use Cases

### Use Case 1: Initial Seeding
**Scenario**: You've added all teams but haven't assigned seeds yet.
- **Seeding Method**: Seed Unseeded Teams
- **Sorting Method**: By Creation Date or By Team Name
- **Seed Placement**: Fill Gaps
- **Gap Closure**: Close All Gaps

### Use Case 2: Adding New Teams
**Scenario**: Tournament has started, some teams are seeded, you're adding new teams.
- **Seeding Method**: Seed Unseeded Teams
- **Sorting Method**: By Creation Date
- **Seed Placement**: At the End
- **Gap Closure**: Let Gaps Remain

### Use Case 3: Complete Re-organization
**Scenario**: You need to completely redo the seeding based on new criteria.
- **Seeding Method**: Reseed All Teams
- **Sorting Method**: Randomly or By Team Name
- **Seed Placement**: N/A (disabled)
- **Gap Closure**: Close All Gaps

### Use Case 4: Random Fair Seeding
**Scenario**: You want unbiased seeding for a new tournament.
- **Seeding Method**: Reseed All Teams
- **Sorting Method**: Randomly
- **Gap Closure**: Close All Gaps

## Validation & Error Handling
- Validates that an active tournament exists
- Validates that selected division exists
- Uses transaction rollback on errors
- Displays error messages via TempData
- Logs all operations and errors

## Success Confirmation
Upon successful seeding:
- User is redirected to the Tournament Teams Index page
- Success message displayed: "Teams seeded successfully in X division(s)."

## Technical Details
- **Controller**: `TournamentTeamsController.AutoSeed` (GET/POST)
- **ViewModel**: `AutoSeedViewModel`
- **View**: `/Areas/Admin/Views/TournamentTeams/AutoSeed.cshtml`
- **Database**: Uses `ApplicationDbContext` with EF Core transactions
- **Logging**: All operations logged via `ILogger<TournamentTeamsController>`

## Code Location
- Controller: `src/VolleyballRallyManager.App/Areas/Admin/Controllers/TournamentTeamsController.cs`
- ViewModel: `src/VolleyballRallyManager.App/Areas/Admin/Models/AutoSeedViewModel.cs`
- View: `src/VolleyballRallyManager.App/Areas/Admin/Views/TournamentTeams/AutoSeed.cshtml`
