# Create Next Round Feature

## Overview

The **Create Next Round** feature provides a simplified, two-step workflow for creating tournament rounds. It leverages the Round entity's recommendation properties to automatically configure rounds with sensible defaults, allowing users to customize settings afterward.

## Purpose

This feature addresses the need for a streamlined round creation process that:
- Reduces setup complexity for new users
- Speeds up round creation with smart defaults
- Maintains flexibility through post-creation customization
- Complements the existing comprehensive "Generate Next Round" feature

## Key Features

✅ **One-Click Round Creation**: Select round type and go
✅ **Smart Defaults**: Automatically applies recommended settings from Round templates
✅ **Non-Playoff Focus**: Only shows regular rounds for streamlined progression
✅ **Edit-First Workflow**: Redirects to Edit page for immediate customization
✅ **Dual Availability**: Accessible from both Details and Index pages

---

## When to Use

### Use "Create Next Round" When:
- You want quick setup with minimal configuration
- You trust the system's recommended defaults
- You prefer to review/adjust settings after creation
- You're creating standard progression rounds (non-playoff)

### Use "Generate Next Round" When:
- You need full control over all settings upfront
- You have specific non-standard requirements
- You want to configure immediate actions (assign teams, generate matches)
- You're experienced with tournament round configuration

---

## User Workflow

### Step-by-Step Process

1. **Navigate to Finalized Round**
   - Go to Tournament Rounds Index or Details page
   - Locate a finalized round (marked with green "Finished" badge)

2. **Initiate Creation**
   - Click the green **"Create Next Round"** button
   - Alternative: Click **"Generate Next Round"** for full configuration

3. **Select Round Type**
   - Review context information (tournament, division, previous round)
   - Choose round type from dropdown (only non-playoff rounds shown)
   - Click **"Create Round"** button

4. **System Creates Round**
   - System retrieves Round template recommendations
   - Calculates defaults based on previous round context
   - Creates TournamentRound with recommended values
   - Redirects to Edit page with success message

5. **Customize Settings (Optional)**
   - Review auto-generated configuration
   - Adjust team selection strategy
   - Modify match generation strategy
   - Configure groups if needed
   - Update advancing teams count

6. **Complete Setup**
   - Save changes if customizations made
   - Assign teams using "Assign Teams Now" checkbox
   - Generate matches using "Generate Matches Now" checkbox

---

## Default Values Logic

### From Round Template (Recommendations)
The system applies the following from the selected Round template:

| Property | Source | Example |
|----------|--------|---------|
| Match Generation Strategy | `Round.RecommendedMatchGenerationStrategy` | RoundRobin, SeededBracket, etc. |
| Team Selection Strategy | `Round.RecommendedTeamSelectionStrategy` | TopByPoints, WinnersOnly, etc. |
| Qualifying Team Selection | `Round.RecommendedTeamSelectionStrategy` | Same as above |
| Is Playoff | `Round.IsPlayoff` | false (filtered out in selection) |

### Calculated from Context
The system calculates the following based on previous round:

| Property | Calculation | Example |
|----------|-------------|---------|
| Qualifying Teams Count | `Round.RecommendedQualifyingTeamsCount` OR `PreviousRound.AdvancingTeamsCount` | 16 teams from previous round |
| Advancing Teams Count | `max(2, QualifyingTeamsCount / 2)` | 8 teams (half of 16) |
| Group Configuration | Based on match strategy | Groups only for GroupStageKnockout |
| Group Value | `max(2, QualifyingTeamsCount / 4)` | 4 groups for 16 teams |

---

## User Interface

### Button Locations

#### Details Page
Located in the action buttons section after "Finalize Round":
```
[Create Next Round] [Generate Next Round] [Create Playoff Round]
    (green)           (outline-green)         (warning)
```

#### Index Page
Located in the Actions column for each finalized round:
```
[Details]
[Create Next Round]
[Generate]
```

### Visual Design
- **Color**: Green (success) to indicate creation action
- **Icon**: Plus circle (bi-plus-circle) for creation
- **Tooltip**: "Quick create with defaults"
- **Contrast**: "Generate Next Round" uses outline style to show alternative

---

## Technical Implementation

### Architecture

```
User Action
    ↓
CreateNextRound (GET)
    ↓
Select Round Type
    ↓
CreateNextRound (POST)
    ↓
Retrieve Round Template
    ↓
Calculate Defaults
    ↓
CreateNextRoundAsync (Service)
    ↓
Redirect to Edit Page
    ↓
Review & Customize
```

### Key Components

#### 1. ViewModel
**File**: `CreateNextRoundSimpleViewModel.cs`
```csharp
public class CreateNextRoundSimpleViewModel
{
    // Context (read-only)
    public Guid TournamentId { get; set; }
    public Guid DivisionId { get; set; }
    public Guid PreviousTournamentRoundId { get; set; }
    
    // User Input (only field)
    [Required]
    public Guid RoundId { get; set; }
}
```

#### 2. Controller Actions
**File**: `TournamentRoundsController.cs`

**GET Action**: Prepares form with non-playoff rounds
**POST Action**: Creates round with defaults, redirects to Edit

#### 3. View
**File**: `CreateNextRound.cshtml`

Simple form with:
- Context information display
- Round type dropdown
- What happens next explanation
- Links to alternative options

### Round Filtering Logic

```csharp
// Only non-playoff rounds shown
var nonPlayoffRounds = await _context.Rounds
    .Where(r => !r.IsPlayoff)
    .OrderBy(r => r.Sequence)
    .ToListAsync();
```

**Rationale**: Playoff rounds require manual team selection and have different workflows. They are created via the dedicated "Create Playoff Round" feature.

---

## Validation & Business Rules

### Preconditions
✅ Previous round must be finalized (`IsFinished == true`)
✅ Previous round must not be locked (`IsLocked == false`)
✅ At least one non-playoff round template must exist

### Validation Rules
✅ Round type selection is required
✅ Selected round must exist in database
✅ Previous round must be accessible
✅ User must have authorization

### Error Handling
- Missing round template → Error message, reload form
- Missing previous round → Error message, redirect to Index
- Validation failure → Display errors, preserve form state

---

## Examples

### Example 1: Standard Progression

**Scenario**: Creating Quarter Finals after Preliminary Round

1. **Previous Round State**:
   - Name: "Preliminary Round"
   - Finalized: Yes
   - Advancing Teams: 16
   - Strategy: Round Robin

2. **User Action**:
   - Clicks "Create Next Round"
   - Selects "Quarter Finals"
   - Submits form

3. **System Creates**:
   - Name: "Quarter Finals"
   - Qualifying Teams: 16 (from previous)
   - Advancing Teams: 8 (half of 16)
   - Match Strategy: Seeded Bracket (from template)
   - Team Selection: Winners Only (from template)

4. **User Customizes** (on Edit page):
   - Reviews settings
   - Keeps defaults
   - Assigns teams
   - Generates matches

### Example 2: Group Stage Round

**Scenario**: Creating a group stage round

1. **Round Template**:
   - Name: "Seeded Round"
   - Recommended Strategy: Group Stage Knockout
   - Recommended Teams: 16

2. **System Calculates**:
   - Groups: 4 (for 16 teams)
   - Configuration: GroupsInRound
   - Value: 4

3. **Result**:
   - Round created with 4 groups
   - User can adjust group count on Edit page

---

## Comparison with Generate Next Round

| Aspect | Create Next Round | Generate Next Round |
|--------|------------------|---------------------|
| **Form Complexity** | Minimal (1 field) | Comprehensive (10+ fields) |
| **Configuration Time** | < 30 seconds | 2-5 minutes |
| **Customization** | After creation (Edit page) | Before creation (form) |
| **Learning Curve** | Low | Moderate |
| **Use Case** | Quick standard setup | Detailed custom setup |
| **Workflow** | Select → Create → Edit | Configure → Create |
| **Immediate Actions** | None (configured on Edit) | Optional (assign/generate) |
| **Round Types** | Non-playoff only | All types |

---

## Best Practices

### For Tournament Organizers

1. **Review Defaults**: Always review auto-generated settings on Edit page
2. **Use Templates**: Ensure Round templates have accurate recommendations
3. **Test First**: Test the workflow with a practice tournament first
4. **Trust but Verify**: Defaults are smart but always confirm before proceeding
5. **Switch When Needed**: Use "Generate Next Round" for complex scenarios

### For System Administrators

1. **Maintain Templates**: Keep Round templates updated with current best practices
2. **Document Strategies**: Ensure staff understand different strategies
3. **Monitor Usage**: Track which feature users prefer
4. **Gather Feedback**: Collect user feedback to improve defaults

---

## Troubleshooting

### Common Issues

#### Issue: "Round must be finalized first"
**Solution**: Finalize the current round before creating next round

#### Issue: "No rounds available in dropdown"
**Solution**: 
- Check if non-playoff round templates exist
- Create Round templates via Admin → Rounds

#### Issue: "Round created with wrong teams"
**Solution**:
- Review qualifying teams count on Edit page
- Adjust team selection strategy
- Re-assign teams if needed

#### Issue: "Groups not configured as expected"
**Solution**:
- Edit round configuration
- Adjust group type and value
- Regenerate matches if needed

---

## Future Enhancements

Potential improvements for future versions:

- [ ] Preview defaults before creation
- [ ] Save custom default preferences per user
- [ ] Quick edit inline without redirecting
- [ ] Template suggestions based on tournament size
- [ ] One-click complete setup (create + assign + generate)
- [ ] Round progression wizard for entire tournament

---

## Related Features

- **Generate Next Round**: Comprehensive round creation
- **Create Playoff Round**: Special round for best losers
- **Create First Round**: Initial round setup
- **Round Templates**: Configurable Round entities
- **Round Recommendations**: Template recommendation system

---

## Technical Notes

### Database Impact
- No schema changes required
- Uses existing Round and TournamentRound tables
- Leverages Round recommendation properties

### Performance
- Single database query for round templates
- Minimal computation for defaults
- Fast redirect to Edit page

### Security
- Requires authentication
- Uses anti-forgery tokens
- Validates user permissions

### Logging
All actions logged via `ILogger<TournamentRoundsController>`:
- Round creation attempts
- Errors during creation
- Template not found errors

---

## Support

For questions or issues:
1. Review this documentation
2. Check [Tournament Rounds Implementation](TOURNAMENT_ROUNDS_IMPLEMENTATION.md)
3. Review [Round Recommendations Feature](ROUND_RECOMMENDATIONS_FEATURE.md)
4. Contact the development team

---

**Last Updated**: 2025-12-11
**Version**: 1.0.0
**Feature Status**: Production Ready
**Author**: ST JAGO Volleyball Rally Development Team