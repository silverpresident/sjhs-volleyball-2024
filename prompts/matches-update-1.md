Implement database changes for "Match Sets" and create four new MVC views/actions for the Matches controller: Assign, Update, Details, and a complex, real-time Scorer interface using SignalR.

---

### 1. Database & Domain Model (Match Set)

Create a new domain entity `MatchSet` and provide the TSQL migration script.

**Entity Specification:**
* Class Name: `MatchSet`
* Inherits from: `BaseEntity`
* Properties:
    * `MatchId` (Guid, Foreign Key to existing Match entity)
    * `SetNumber` (int)
    * `HomeTeamScore` (int)
    * `AwayTeamScore` (int)
    * `IsFinished` (bool) - Indicates if the set has concluded.
    * `IsLocked` (bool) - Indicates that the scores for this set are finalized and cannot be edited except by an admin.

**Database Constraints:**
* Create a unique composite index on `(MatchId, SetNumber)`.
* Ensure appropriate Foreign Key constraints exist back to the `Matches` table.

**Existing Context Note:** The main `Match` entity already contains an integer field `CurrentSetNumber` which needs to be managed during set transitions.

---

### 2. Database & Domain Model (Match)
Update the Match entity to include 2 fields
   * `CurrentSetNumber` (int) default 0
    * `IsLocked` (bool) - Indicates that the scores for this match are finalized and cannot be edited except by an admin.

---

### 3. Matches Controller - Standard Views

#### A. Assign Action (GET/POST /Matches/Assign/{id})
* **View Goal:** Edit logistical details: Date/Time, Court ID (dropdown), Referee Name, Scorer Name.
* **POST Logic:** Compare posted values with DB. If changed, update the Match entity and create a `MatchUpdate` summarizing the changes.
* **Index View:** Add an "Assign" button. Link existing Time and Court fields on the index to this view.

#### B. Update Action (GET/POST /Matches/Update/{id})
* **View Goal:** Simple form with a text area to submit a general match commentary update.
* **POST Logic:** Create a `MatchUpdate` entity with the submitted text.

#### C. Details View (GET /Matches/Details/{id})
* **Additions:** Below main details, add a table showing related `MatchSet` records.
* **Set Editing:**
    * Include an "Edit" option for each set history row.
    * **Constraint:** The "Edit" option must be disabled/hidden if `MatchSet.IsLocked` is true.
    * **POST Logic:** Updating a past set score must update the `MatchSet` record, generate a `MatchUpdate`, and recalculate the parent Match entity's overall score based on sets won.

---

### 3. The Scorer View (Real-time Interface)

**Route:** `GET /Matches/Scorer/{id}`

**Overview:** A single-page real-time application using SignalR. It must synchronize state across multiple simultaneous scorers.

**Visual Layout (Refer to image_1.png):**
Divide the page into 3 distinct areas based on the blue outlines in the image:
1.  **Main Scoring Area:** Large central/left area containing team scores and center controls.
2.  **Quick Actions:** Bottom left banner.
3.  **Update Feed:** Right-hand sidebar showing the 15 most recent `MatchUpdate` entries.

#### SignalR Strategy (ScorerHub)

The Hub handles broadcasting scores, feed updates, and **set state transitions**.

**Server-side Hub Methods (called by client JS):**
* **`SendScoreUpdate(matchId, setNumber, teamId, incrementValue)`:** Updates score for the specific un-locked match/set combo and broadcasts.
* **`SendQuickAction(matchId, actionType)`:** Handles Call, Start, End Match, Dispute.
* **`SendSetStateChange(matchId, actionType)`:**
    * `actionType` = "EndCurrentSet": Mark current set Finished.
    * `actionType` = "StartNextSet": Increment Match.CurrentSetNumber, create new set.
    * `actionType` = "RevertToPreviousSet": Decrement Match.CurrentSetNumber, set current set Finished to false.
    * *Crucial:* Broadcast these state changes to synchronize UI elements (buttons shown, labels).

**Client-side Event Listeners (broadcast from server):**
* `ReceiveScoreUpdate`: Updates large scores and history list.
* `ReceiveMatchStateChange`: Updates overall status and Quick Action availability.
* `ReceiveFeedUpdate`: Updates feed sidebar.
* `ReceiveSetStateChange`: Updates current set label, refreshes sets-won counter, updates history list, and toggles center button visibility.

#### Main Scoring Area UI Behavior (Center/Left)

*Use match-scorer-sketch.png as the visual guide.*

**1. Team Score Panels (Left & Right Green/Yellow areas):**
* Display Team Names and large digital current scores.
* **[+] / [-] Buttons:** Clicking these calls SignalR `SendScoreUpdate` for the *current* active set number.

**2. Center Set Controls (Central stack in image_1.png):**
* **Set Label:** Display current label (e.g., "SET 3") at the top center.
* **"previous set" Button:**
    * *Constraint:* Active only if the current set score is exactly 0-0 AND it is not the first set.
    * *Action:* Triggers SignalR `SendSetStateChange("RevertToPreviousSet")`.
* **"end set" Button:**
    * *Constraint:* Active only if the current set is in progress.
    * *Action:* Triggers SignalR `SendSetStateChange("EndCurrentSet")`.
* **"new set" Button:**
    * *Constraint:* Active only if the current set has just finished.
    * *Action:* Triggers SignalR `SendSetStateChange("StartNextSet")`.

**3. Scoring History (Bottom Center):**
* **Sets Won Counter:** Visual display (the [0]|[1] boxes) of total sets won.
* **Set History List:** Displays completed sets (e.g., "set 1: 25 v 6").

#### Quick Actions Banner (Bottom Left)
* **CALL TO COURT:** Generate `MatchUpdate`.
* **MATCH STARTED:** Update start time, status to In Progress.
* **MATCH ENDED:** Set `IsFinished`, calculate final sets won, update parent match score, lock all sets.
* **DISPUTED:** Toggle status.