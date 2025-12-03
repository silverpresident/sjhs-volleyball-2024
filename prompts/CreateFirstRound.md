## ✍️ Improved Prompt: `CreateFirstRound` Feature Enhancement

**Goal:** Enhance the "Create First Round" functionality and introduce a new "Edit Round" feature in the Admin Area `TournamentRoundsController`. This feature will manage the initial setup and group/match structure of the first round.

---

### 1. 🏗️ Controller and Security Requirements

* **Controller:** All actions reside in the Admin Area's `TournamentRoundsController`.
* **Access Guard:** The `CreateFirstRound` GET action **must** check the database. If a round with a `Sequence > 1` already exists for the current division, the user must be shown an **error warning** and redirected (e.g., to the `Index` view or the `Edit` view for Round 1). **Access is refused** if this is not the first round.
* **Security:** Ensure all POST actions (`CreateFirstRound` and `Edit`) use the **Anti-Forgery Token** (`[ValidateAntiForgeryToken]`) for security (C# MVC requirement).

---

### 2. 📝 `CreateFirstRound` Action and View Modification

The GET and POST actions for `CreateFirstRound` must be updated as follows:

* **Display Data:** The view must clearly display the **Tournament Name**, **Division Name**, and the current **Round Information** retrieved from the `Round` Entity where `Sequence = 1`.
* **Round Selection:** The round input must be **non-selectable** (disabled/hidden input) and **fixed** to `Round.Sequence = 1`.
* **Group Configuration Input:**
    * Implement a single **integer number input** paired with an **select dropdown**.
    * The user must actively select **one** of the two options from the dropdown:
        * "Teams per group"
        * "Groups in round"
    * The integer input field **must have a minimum value of 2** and a maximum value of half the number of teams in the division plus 1.
* **Existing Fields:**
    * Retain the input for **Team Selection Method** (from `TeamSelectionMethod` enum).
    * Retain the input for **Generation Strategy** (from `MatchGenerationStrategy` enum).
* **Post-Action Flags (Checkboxes):**
    * **Assign Teams:** An option (e.g., a checkbox) to **assign teams now** or **defer to later**.
    * **Generate Matches:** An option (e.g., a checkbox) to **generate matches now** or **defer to later**.

---

### 3. 💾 `TournamentRound` Entity and Database Update

Modify the **`TournamentRound` Entity** (and the corresponding database table) to store the following new integer properties:

* `TeamsPerGroup`
* `GroupsInRound`

The POST actions should correctly persist the selected value for whichever option the user picked in the Group Configuration Input.

---

### 4. ✏️ `Edit` Action and View Implementation

Introduce a new `Edit` action (GET and POST) for the `TournamentRound` entity to allow modification of an existing round's settings. Add the edit button to TournamentRounds Details page.

* **Updatable Fields:** The Edit action should allow the user to modify the following existing properties of the `TournamentRound`:
    * **Group Configuration:** The same select/integer input pair used in `CreateFirstRound` (**TeamsPerGroup** or **GroupsInRound**).
    * **Team Selection Method**
    * **Generation Strategy**
    * **Assign Teams Flag** (now/defer)
    * **Generate Matches Flag** (now/defer)

---

### 5. 🚀 Immediate Execution Workflow Notes

When the `CreateFirstRound` or `Edit` form is submitted (POST):

* **If 'Assign teams now' is selected:** Immediately execute the **`SelectTeams` workflow** (which should be an internal service/method call).
* **If 'Generate match now' is selected:** After saving the round configuration, redirect the user immediately to the **`GenerateMatches` view/action** for the newly configured round.

### 6. Button states
The TournamentRounds Details page has a number of hidden buttons.
These buttons should be visible but disabled rather than hidden.

