## 🏆 Tournament Round Management Feature (Text-Only Prompt)

Implement the necessary data model changes, backend services, and controllers for managing the full lifecycle of a tournament round, from creation to finalization.

### 1. Data Model Requirements and Context 📝

**Crucial Context:** The tournament contains **multiple divisions**, and the rounds are sequenced **independently within each division**.

Ensure the following entities are correctly defined and related:

* **`TournamentRoundTeam` Entity:** Must inherit from BaseEntity. Must include foreign keys for the four primary identifiers, and the two rank properties:
    * `TournamentId`, `DivisionId`, `RoundId`, `TeamId`
    * `SeedNumber` (int, the incoming rank from the previous round/initial seeding)
    * `FinalRank` (int, the final rank/position achieved by the team in this specific round)
    * Points (int)     
    * SetsFor (int)     
    * SetsAgainst (int)     
    * SetsDifference (int, derived: SetsFor - SetsDifference)
    * ScoreFor (int)     
    * ScoreAgainst (int)     
    * ScoreDifference (int, derived: ScoreFor - ScoreAgainst)
Copy relevant properties from  TournamentDivisionTeam  entity

* **`TournamentRound` Entity:** Must inherit from BaseEntity. (As described, ensure the custom properties are mapped correctly):
    * `TournamentId`, `DivisionId`, `RoundId` (Primary Identifiers)
    * `RoundNumber` (int, sequenced within the Division)
    * `TeamSelectionMethod` (Enum: e.g., 'Manual', 'SeedTopHalf', 'WinnersOnly',Look at Admin/Views/Matches/AutoGenerateNextRound.cshtml)
    * MatchGenerationStrategy (Enum: e.g., 'RoundRobin', 'SeededBracket', 'Manual')
    * `PreviousTournamentRoundId` (Nullable Guid)
    * `TeamsAdvancing` (int, number of teams advancing to the next stage)
    * `IsFinished` (bool, default `false`)
    * `IsLocked` (bool, default `false`)

Create TSQL scripts to generate the db tables
---

### 2. Service Logic: `RanksService`

Create a dedicated service to handle ranking updates.

* **`UpdateTeamRanks(roundId)`:**
    * Iterate through all matches in the specified round.
    * Calculate points for each team based on match results: **Win (3 pts), Draw (1 pt), Loss (0 pts)**.
    * Update the team's internal score/point total for that round.
    * Determine the **`FinalRank`** for each `TournamentRoundTeam` based on points (and any required tie-breakers like goal difference).
    * using the following strict tie-breaker rules: 
          1. Highest Total Points 
          2. Highest Score Difference (ScoreDifference) 
          3. Highest Score For (ScoreFor) 
          4. Best Seed Number (The team with the lower SeedNumber gets the better rank)


---

### 3. Controller & UI (Index View Conditional Logic) 🚦

Implement the `TournamentRound` index view, listing all rounds. **The visibility of action buttons for each round must follow these strict conditional rules:**

* **Details Button:** Always display this button. It should navigate to the details page, which must include the list of teams, their **`SeedNumber`**, and their **`FinalRank`**.
* **Finalize Round Button:** Only display this button if the round's state is **`IsFinished = false` AND every single match within that round is recorded as complete**. Clicking this button executes the ranking service and sets the round's state to `IsFinished = true` and `IsLocked = true`.
* **Select Teams Button:** Only display this button if the round has been created but **no teams have been assigned yet**, AND the **`PreviousTournamentRound` has `IsFinished = true`**. Clicking this runs the team selection and seeding logic, sets the previous round's `IsLocked = true`, and then **immediately redirects to the Generate Matches workflow**.
* **Generate Matches Button:** Only display this button if teams have been successfully selected and seeded for the current round, but **no matches have been generated yet**. Clicking this generates all match entities for the round and then **immediately redirects to the Details page for the current round**.
* **Generate Next Round Button:** Only display this button if the current round's state is **`IsFinished = true`** and teams have been successfully selected/seeded. Clicking this creates the subsequent round entity and then **immediately redirects to the 'Select Teams' workflow for that newly created round**. (look at `Admin/Views/Matches/AutoGenerateNextRound.cshtml` for the expected behaviour)

**Global Buttons:** Implement two global action buttons: **Create First Round** (look at `Admin/Views/Matches/AutoGenerateFirstRound.cshtml` for the expected behaviour).

---

### 4. Workflow Implementation Details (Backend Services)

The following steps must be implemented as distinct service methods to control the flow.

#### 4.1. 🆕 Create First Round (behaves like AutoGenerateFirstRound.cshtml)
1.  **Round Creation:** Create the `TournamentRound` entity (Setting `PreviousTournamentRoundId` to null).
2.  **Team Insertion:** Fetch **all** available teams from the targeted division/group.
3.  For each team, create a `TournamentRoundTeam` entry, assigning an initial **`SeedNumber`** (e.g., based on group or previous division rank). Initial seeding for the first round must be based on the current SeedNumber from the TournamentDivisionTeam entity.
4.  Do NOT create matches.

#### 4.2. ✅ Finalize Round
1.  Verify all matches in the round are complete.
2.  Execute **`RanksService.UpdateTeamRanks(roundId)`** to calculate and store the `FinalRank`.
3.  Mark the `TournamentRound` as **`IsFinished = true`**.
4.  Update global team point totals based on the results of this round.

#### 4.3. ➡️ Generate Next Round (Targets AutoGenerateNextRound.cshtml)
1.  Create the new `TournamentRound` entity. Select a roud from the list of rounds in the system.
2.  Set its `PreviousTournamentRoundId` to the current (finalized) round's ID.
3.  Do NOT add teams or matches yet.
4.  After creation, **redirect to the 'Select Teams' workflow** for the newly created round.

#### 4.4. 🔢 Select Teams
1.  **Pre-Condition Check:** Ensure `PreviousTournamentRound` exists and `IsFinished` is `true`.
2.  **Selection Logic:**  Based on the round's SelectionMethod and TeamsAdvancing:
    * Fetch the teams from the previous round with the best FinalRank. The selection logic must respect the SelectionMethod (e.g., WinnersOnly implies only teams with FinalRank = 1 qualify, up to TeamsAdvancing). If SelectionMethod is 'SeedTopHalf', it takes the best N teams where N is half the number of teams in the previous round, capped by TeamsAdvancing.
3.  **Team Insertion & Seeding:**
    * For each qualifying team, create a `TournamentRoundTeam` record in the new round.
    * Assign the `FinalRank` from the previous round as the new round's **`SeedNumber`**.
    * The lowest FinalRank (1) must become the lowest SeedNumber (1) in the new round.
4.  **Locking:** Set the previous round's `IsLocked = true`.
5.  After selection, **redirect to the 'Generate Matches' workflow**.

#### 4.5. 🧩 Generate Matches
1.  Using the list of seeded teams (`TournamentRoundTeam` entries) in the current round, implement the appropriate match generation algorithm based on the **`TournamentRound.MatchGenerationStrategy`** property.
2.  **Default/Fallback Strategy:** If the strategy is not explicitly set, assume **Round-Robin** if the number of teams is small (e.g., < 10), otherwise assume **Seeded Bracket**.
3.  **Seeded Bracket Generation:** If a bracket is required, the generation must strictly adhere to **Standard Tournament Seeding** (e.g., 1st  Seed vs Last Seed, 2nd Seed vs Second-to-Last Seed, etc.) using the team's **`SeedNumber`**.
4.  Create all required `Match` entities for this round.
5.  After match generation, **redirect to the `TournamentRound/Details` view** for the current round.

### Documentation
Make sure to write and display proper documentation for this feature.
