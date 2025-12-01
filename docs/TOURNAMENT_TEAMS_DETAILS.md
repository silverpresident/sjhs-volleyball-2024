# Tournament Teams Details Page

## Overview
The Tournament Teams Details page displays comprehensive information about a team participating in the active tournament. This page shows team information, tournament statistics, and complete match history.

## Implementation Details

### Files Created/Modified

1. **TournamentTeamDetailsViewModel.cs** (Created)
   - Location: `src/VolleyballRallyManager.App/Areas/Admin/Models/TournamentTeamDetailsViewModel.cs`
   - Properties:
     - `TournamentTeamDivision`: The team's tournament participation record
     - `Matches`: List of all matches for the team
     - `Division`: The division the team is playing in

2. **TournamentTeamsController.cs** (Modified)
   - Location: `src/VolleyballRallyManager.App/Areas/Admin/Controllers/TournamentTeamsController.cs`
   - Added `Details(Guid id)` action method
   - Added logging support with `ILogger<TournamentTeamsController>`
   - Features:
     - Retrieves team information from active tournament
     - Fetches all matches for the team
     - Includes error handling with try-catch
     - Returns 404 if team or tournament not found

3. **Details.cshtml** (Modified)
   - Location: `src/VolleyballRallyManager.App/Areas/Admin/Views/TournamentTeams/Details.cshtml`
   - Updated to use `TournamentTeamDetailsViewModel` instead of `TournamentTeamDivision`
   - Uses green and gold theme (project colors)

## Features

### Team Information Card
Displays:
- Team name (as page title)
- School name
- Division
- Group name (or "N/A" if not assigned)
- Seed number
- Team color indicator (circular badge)
- Team logo (if available)

### Tournament Statistics Card
Shows comprehensive statistics split into two columns:

**Column 1 - Match Results:**
- Matches Played (bold)
- Wins (green badge)
- Draws (secondary badge)
- Losses (red badge)

**Column 2 - Points:**
- Total Points (bold, green text)
- Points Scored
- Points Conceded
- Point Difference (green for positive, red for negative, with +/- prefix)

### Match History Table
Features:
- Responsive table design
- Shows all matches for the team (home and away)
- Ordered by scheduled time (most recent first)
- Color-coded rows:
  - Green background for wins
  - Red background for losses
  - Yellow background for draws
  - Default for scheduled matches

**Columns:**
- Date & Time (formatted as "Ddd, dd MMM yyyy HH:mm")
- Round name
- Home Team (bold if it's the current team)
- Score (or "vs" for scheduled matches)
- Away Team (bold if it's the current team)
- Court location
- Status badge:
  - "Won" (green) for victories
  - "Lost" (red) for defeats
  - "Draw" (yellow) for ties
  - "Scheduled" (blue) for upcoming matches

### Action Buttons
- **Edit Team**: Links to the Edit action with team ID
- **Back to Teams List**: Returns to the TournamentTeams Index page

## Usage

### Accessing the Page
Navigate to: `/Admin/TournamentTeams/Details/{teamId}`

Where `{teamId}` is the Guid of the team.

### From Other Pages
The Details page can be accessed from:
- TournamentTeams Index page (via Details link for each team)
- Any page that has a link to view team details

## Error Handling

The controller includes comprehensive error handling:
1. Checks if an active tournament exists
2. Validates that the team exists in the tournament
3. Logs warnings for missing data
4. Logs errors for exceptions
5. Returns appropriate HTTP status codes:
   - 404 for missing tournament or team
   - 500 for server errors

## Design
- Uses Bootstrap 5 classes for responsive layout
- Green headers (`bg-success text-white`) to match project theme
- Card-based layout for organized sections
- Responsive table for match history
- Bootstrap Icons for action buttons

## Notes
- The page only displays data for the active tournament
- Match history includes both home and away matches
- Statistics are automatically calculated and stored in the `TournamentTeamDivision` entity
- The page uses the team's color for the color indicator in the header
