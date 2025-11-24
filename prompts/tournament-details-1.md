I need to update the "Tournament Details" feature. Please perform the following steps. 

### 1. Create View Models
Create a new file for `TournamentDetailsViewModel` and its supporting classes. 
The structure should be:

**TournamentDetailsViewModel**
- `Guid TournamentId`
- `Tournament` (The existing entity)
- `List<TournamentDivisionViewModel> Divisions`
- `List<TournamentRoundViewModel> Rounds`
- `List<TournamentTeamDivision> Teams`

**TournamentDivisionViewModel**
- Should map properties from the existing `TournamentDivision` entity.
- `string GroupName` 
- `int TeamCount` (Logic: Count of teams in this division)
- `int MatchesPlayed` (Logic: Count of matches in this division with a completed status)

**TournamentRoundViewModel**
- `string DivisionName`
- `string RoundName`
- `int TeamCount`
- `int MatchesScheduled` (Logic: Total matches in this round)
- `int MatchesPlayed` (Logic: Completed matches in this round)

### 2. Update Controller/Logic
Update the Controller that handles `Tournament/Details` and `ActiveTournament/Index`. 
- Populate the `TournamentDetailsViewModel`. 
- Ensure you write the LINQ queries or logic required to calculate the counts for `TeamCount`, `MatchesScheduled`, and `MatchesPlayed`.

### 3. Update Views
Update the `Tournament/Details.cshtml` and `ActiveTournament/Index.cshtml` views to use the new ViewModel.
- **Divisions Section:** Create a responsive table listing the divisions with their calculated stats.
- **Rounds Section:** Create a separate section (or tab) displaying the round information.