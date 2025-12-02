# Tournament Rounds - Rank Teams Feature

## Overview
This document describes the implementation of the Rank Teams feature in the Tournament Rounds Details page.

## Changes Made

### 1. Controller Updates (`TournamentRoundsController.cs`)

#### Added Dependencies
- Injected `IRanksService` to enable team ranking functionality

#### New Action: RankTeams
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> RankTeams(Guid id)
```

**Functionality:**
- Updates team rankings based on current match results
- Validates that the round is not finalized
- Uses `IRanksService.UpdateTeamRanksAsync()` to calculate rankings
- Returns success/error messages via TempData
- Redirects back to round details

**Business Rules:**
- Only works on non-finalized rounds
- Requires matches to exist in the round
- Updates rankings based on:
  1. Total Points (Win=3, Draw=1, Loss=0)
  2. Score Difference
  3. Score For
  4. Seed Number (tie-breaker)

### 2. View Updates (`Details.cshtml`)

#### Added Disputed Matches Display
In the "Round Statistics" section:
- Shows count of disputed matches
- **Red badge with warning icon** when disputes exist
- **Green badge with check icon** when no disputes
- Larger font size (fs-6) for visibility

```razor
<dt class="col-sm-5">Disputed Matches:</dt>
<dd class="col-sm-7">
    @if (disputedCount > 0)
    {
        <span class="badge bg-danger fs-6">
            <i class="bi bi-exclamation-triangle"></i> @disputedCount
        </span>
    }
    else
    {
        <span class="badge bg-success">
            <i class="bi bi-check-circle"></i> 0
        </span>
    }
</dd>
```

#### Added Action Buttons
New buttons in the action button group:

1. **Rank Teams Button**
   - Icon: `bi-sort-numeric-down`
   - Color: Info (blue)
   - Condition: Round not finished AND matches exist
   - Confirmation prompt before execution
   - Posts to `RankTeams` action

2. **Generate Next Round Button** (already existed)
   - Icon: `bi-arrow-right-circle`
   - Color: Secondary (gray)
   - Condition: Round is finished
   - Links to `GenerateNextRound` action

## Button Visibility Logic

The action buttons appear based on round state:

| Button | Visibility Condition |
|--------|---------------------|
| Select Teams | No teams assigned AND previous round exists |
| Generate Matches | Teams assigned AND no matches generated |
| **Rank Teams** | Round not finished AND matches exist |
| Finalize Round | All matches complete AND not finalized |
| Generate Next Round | Round is finalized |

## User Workflow

### Typical Round Progression:
1. Create round
2. Select teams (auto or manual)
3. Generate matches
4. Matches are played
5. **[NEW] Click "Rank Teams"** to update standings mid-round
6. All matches complete
7. Click "Finalize Round" (also ranks teams)
8. Click "Generate Next Round"

### When to Use Rank Teams:
- **During a round** to see current standings
- After some (but not all) matches are complete
- To verify ranking calculations before finalizing
- **Cannot be used** after round is finalized

## UI Improvements

### Disputed Matches Highlight
- Prominently displayed in Round Statistics card
- Visual distinction between zero disputes (green) and existing disputes (red)
- Helps administrators quickly identify rounds needing attention

### Rank Teams Button
- Positioned logically between "Generate Matches" and "Finalize Round"
- Info color distinguishes it from final actions
- Confirmation dialog prevents accidental clicks

## Technical Notes

### Service Dependencies
- `IRanksService`: Calculates and updates team rankings
- `ITournamentRoundService`: Manages round state and validation
- `ILogger`: Logs errors for troubleshooting

### Error Handling
- Validates round exists
- Prevents ranking finalized rounds
- Catches and logs exceptions
- User-friendly error messages via TempData

### Security
- Requires authentication (`[Authorize]`)
- CSRF protection via `[ValidateAntiForgeryToken]`
- Confirmation dialogs prevent accidental actions

## Testing Considerations

### Test Scenarios:
1. Rank teams with no matches (should not appear)
2. Rank teams with some matches complete
3. Rank teams with all matches complete
4. Attempt to rank finalized round (should show error)
5. Verify disputed matches badge shows correct count
6. Verify disputed matches badge color changes

### Expected Behavior:
- Rankings update immediately
- Success message confirms action
- Team standings table reflects new rankings
- FinalRank column populates with calculated values

## Files Modified

1. `src/VolleyballRallyManager.App/Areas/Admin/Controllers/TournamentRoundsController.cs`
   - Added IRanksService dependency injection
   - Implemented RankTeams action method

2. `src/VolleyballRallyManager.App/Areas/Admin/Views/TournamentRounds/Details.cshtml`
   - Added disputed matches count display
   - Added Rank Teams button
   - Updated action button layout

## Related Documentation

- [Tournament Rounds Implementation](./TOURNAMENT_ROUNDS_IMPLEMENTATION.md)
- [Tournament Rounds User Guide](./TOURNAMENT_ROUNDS_USER_GUIDE.md)
- [Tournament Rounds Final Status](./TOURNAMENT_ROUNDS_FINAL_STATUS.md)

## Color Scheme Reference

Following project theme (green and gold):
- Success/Complete: Green (`bg-success`)
- Warning/Pending: Gold/Yellow (`bg-warning`)
- Error/Disputed: Red (`bg-danger`)
- Info/Action: Blue (`bg-info`)
- Secondary: Gray (`bg-secondary`)
