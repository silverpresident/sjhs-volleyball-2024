# Round Recommendations Feature

## Overview
This feature adds recommendation properties to the **Round** entity (template) to guide tournament organizers when creating **TournamentRound** instances. The Round entity now serves as a smart template with suggested defaults that can be customized per tournament.

## What Changed

### 1. Database Schema Changes
**File:** [`database/migration-add-round-recommendations.sql`](../database/migration-add-round-recommendations.sql)

Added three new columns to the `Rounds` table:
- `RecommendedMatchGenerationStrategy` (INT) - Suggests best match generation format
- `RecommendedTeamSelectionStrategy` (INT) - Suggests how to rank/select teams
- `IsPlayoff` (BIT) - Indicates if this is a playoff/elimination round

Renamed column:
- `QualifyingTeams` → `RecommendedQualifyingTeamsCount` (for clarity)

### 2. Entity Model Updates
**File:** [`src/VolleyballRallyManager.Lib/Models/Round.cs`](../src/VolleyballRallyManager.Lib/Models/Round.cs)

```csharp
public class Round : BaseEntity
{
    public required string Name { get; set; }
    public required int Sequence { get; set; }
    
    // NEW: Recommendation properties
    public int RecommendedQualifyingTeamsCount { get; set; } = 0;
    public MatchGenerationStrategy RecommendedMatchGenerationStrategy { get; set; } = MatchGenerationStrategy.RoundRobin;
    public TeamSelectionStrategy RecommendedTeamSelectionStrategy { get; set; } = TeamSelectionStrategy.TopByPoints;
    public bool IsPlayoff { get; set; } = false;
    
    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
}
```

### 3. Controller Updates
**File:** [`src/VolleyballRallyManager.App/Areas/Admin/Controllers/RoundsController.cs`](../src/VolleyballRallyManager.App/Areas/Admin/Controllers/RoundsController.cs)

Added full CRUD operations:
- **Create** - Create new round templates
- **Edit** - Modify existing round templates  
- **Delete** - Remove round templates
- **Index** - Enhanced to display new properties

### 4. View Models
**File:** [`src/VolleyballRallyManager.App/Areas/Admin/Models/RoundViewModel.cs`](../src/VolleyballRallyManager.App/Areas/Admin/Models/RoundViewModel.cs)

Added `CreateEditRoundViewModel` for Create/Edit operations:
```csharp
public class CreateEditRoundViewModel
{
    public Guid? Id { get; set; }
    [Required] public string Name { get; set; }
    [Required] public int Sequence { get; set; }
    public int RecommendedQualifyingTeamsCount { get; set; }
    public MatchGenerationStrategy RecommendedMatchGenerationStrategy { get; set; }
    public TeamSelectionStrategy RecommendedTeamSelectionStrategy { get; set; }
    public bool IsPlayoff { get; set; }
}
```

Updated `RoundViewModel` to include new properties for display.

### 5. User Interface
**Files:**
- [`src/VolleyballRallyManager.App/Areas/Admin/Views/Rounds/Index.cshtml`](../src/VolleyballRallyManager.App/Areas/Admin/Views/Rounds/Index.cshtml)
- [`src/VolleyballRallyManager.App/Areas/Admin/Views/Rounds/Create.cshtml`](../src/VolleyballRallyManager.App/Areas/Admin/Views/Rounds/Create.cshtml)
- [`src/VolleyballRallyManager.App/Areas/Admin/Views/Rounds/Edit.cshtml`](../src/VolleyballRallyManager.App/Areas/Admin/Views/Rounds/Edit.cshtml)

Enhanced UI features:
- Create/Edit forms with all recommendation fields
- Visual indicators for playoff rounds (trophy badge)
- Display of recommended strategies  
- Delete functionality with confirmation
- Helpful tooltips explaining each field

### 6. Database Initialization
**File:** [`src/VolleyballRallyManager.Lib/Configuration/DatabaseInitialization.cs`](../src/VolleyballRallyManager.Lib/Configuration/DatabaseInitialization.cs)

Updated seed data with intelligent defaults:

| Round Name | Sequence | Recommended Strategy | Team Selection | Is Playoff |
|-----------|----------|---------------------|----------------|------------|
| Preliminary Round | 1 | Round Robin | Top By Points | No |
| Seeded Round | 2 | Group Stage Knockout | Top By Points | No |
| Quarter Finals | 4 | Seeded Bracket | Winners Only | Yes |
| Semi Finals | 5 | Seeded Bracket | Winners Only | Yes |
| Third Place Playoff | 6 | Seeded Bracket | Winners Only | Yes |
| Finals | 7 | Seeded Bracket | Winners Only | Yes |

## Architecture

### Conceptual Model
```
Round (Template)                    TournamentRound (Instance)
├─ Name                            ├─ Round (reference)
├─ Sequence                        ├─ Tournament
├─ RecommendedQualifyingTeams      ├─ Division
├─ RecommendedMatchStrategy    ──► ├─ MatchGenerationStrategy
├─ RecommendedTeamSelection    ──► ├─ AdvancingTeamSelectionStrategy
└─ IsPlayoff                   ──► └─ IsPlayoff
```

**Key Principle:** Round = reusable template with recommendations, TournamentRound = specific instance that can override those recommendations.

## Usage Workflow

### For Administrators

1. **View Round Templates**
   - Navigate to Admin → Rounds
   - See all configured round templates with their recommendations

2. **Create New Round Template**
   - Click "Create New Round"
   - Enter round details:
     * Name (e.g., "Quarter Finals")
     * Sequence (order number)
     * Recommended match generation strategy
     * Recommended team selection method
     * Recommended qualifying teams count
     * Mark as playoff round if applicable

3. **Edit Existing Template**
   - Click "Edit" on any round card
   - Modify recommendations as needed
   - Save changes

4. **Delete Template**
   - Click "Delete" with confirmation
   - Note: Cannot delete rounds in use by tournaments

### For Tournament Organizers

When creating tournament rounds, the system will:
1. Show recommendations from the selected Round template
2. Allow customization/override of all settings
3. Use recommendations as smart defaults to speed up setup

## Benefits

✅ **Consistency** - Standard recommendations across tournaments  
✅ **Flexibility** - Per-tournament customization when needed  
✅ **Guidance** - Helps new organizers make informed decisions  
✅ **Efficiency** - Smart defaults reduce setup time  
✅ **Clarity** - Clear distinction between playoff and regular rounds

## Migration Instructions

### For Existing Databases

1. **Backup your database** before running migrations

2. **Run the migration script:**
   ```sql
   -- Execute from SQL Server Management Studio or Azure Data Studio
   -- File: database/migration-add-round-recommendations.sql
   ```

3. **Verify the migration:**
   ```sql
   SELECT Name, Sequence, RecommendedQualifyingTeamsCount, 
          RecommendedMatchGenerationStrategy, 
          RecommendedTeamSelectionStrategy, IsPlayoff
   FROM Rounds
   ORDER BY Sequence;
   ```

4. **Update application:**
   - Deploy updated code
   - Restart application

### For New Installations

The new schema and seed data will be applied automatically during database initialization.

## API Reference

### Enums

**MatchGenerationStrategy**
```csharp
public enum MatchGenerationStrategy
{
    RoundRobin = 0,          // All teams play each other
    SeededBracket = 1,       // Elimination bracket
    Manual = 2,              // Manually create matches
    Swiss = 3,               // Swiss system
    GroupStageKnockout = 4   // Group stage then knockout
}
```

**TeamSelectionStrategy**
```csharp
public enum TeamSelectionStrategy
{
    Manual = 0,                    // Manual selection
    TopByPoints = 1,               // Best overall records
    SeedTopHalf = 2,               // Top 50% advance
    WinnersOnly = 3,               // Only rank 1 teams
    TopFromGroupAndNextBest = 4    // Best from each group plus wildcards
}
```

## Technical Notes

### Property Naming
- Used "Recommended" prefix to make it clear these are suggestions
- Kept property names consistent with TournamentRound entity

### Enum Storage
- Enums are stored as **strings** in the database (NVARCHAR(50))
- Entity Framework Core uses `.HasConversion<string>()` for automatic conversion
- This approach provides better readability in database queries and is consistent with other enums in the project
- Example values: 'RoundRobin', 'SeededBracket', 'TopByPoints', 'WinnersOnly'

### Backward Compatibility
- Existing code continues to work
- Migration provides sensible defaults for existing rounds
- No breaking changes to existing APIs

### Validation
- All required fields have validation attributes
- Sequence must be 1-100
- Qualifying teams count must be 0-100 (0 = all teams)

## Future Enhancements

Potential improvements for future versions:
- [ ] Copy round templates between tournaments
- [ ] Import/export round configurations
- [ ] Round template versioning
- [ ] Validation rules per strategy type
- [ ] Template categories (e.g., "Youth League", "Professional")

## Related Documentation

- [Tournament Rounds Implementation](TOURNAMENT_ROUNDS_IMPLEMENTATION.md)
- [Tournament Rounds User Guide](TOURNAMENT_ROUNDS_USER_GUIDE.md)
- [Rounds Index Improvements](ROUNDS_INDEX_IMPROVEMENTS.md)

## Support

For questions or issues:
1. Check existing documentation
2. Review the .clinerules file for coding standards
3. Contact the development team

---

**Last Updated:** 2025-12-11  
**Version:** 1.0.0  
**Author:** ST JAGO Volleyball Rally Development Team