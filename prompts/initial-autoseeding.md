## ✍️ AutoSeed Feature  

**Goal:** Implement a new **AutoSeed** feature in the `TournamentTeamsController` within the admin area. This feature handles seeding/re-seeding of teams within divisions based on user-defined criteria.

---

### 1. 🏗️ Controller and View Requirements

* **Controller:** In the admin area, in the `TournamentTeamsController`, create a new action: `AutoSeed` (GET and POST).
* **Index View Link:** On the `TournamentTeams Index` page, place a link to the new `AutoSeed` view. The link should be styled as a button and located next to the existing "Add Team" button.
* **Security:** The `AutoSeed` POST action **must** be protected using the **Anti-Forgery Token** (`[ValidateAntiForgeryToken]`) for security, as this is a C# MVC project.

---

### 2. 📝 AutoSeed Form Options

The `AutoSeed` form will allow the user to define the scope and logic of the seeding process. **All multi-choice options must be rendered using HTML Radio Buttons**.

* **Division Selection:**
    * A Select Dropdown must be provided to choose the scope.
    * Options: Select a single **Division** (from the active tournament) OR select the option **"All Divisions"**.
* **Seeding Method (Radio Inputs):** Defines which teams are affected.
    * **"Seed Unseeded Teams"** (Default): Only teams with a current `SeedNumber` of **0 or NULL** will be assigned a new seed.
    * **"Reseed All Teams"**: All teams in the selected division(s), regardless of their current seed, will be re-assigned new seed numbers.
* **Sorting Method (Radio Inputs):** Defines the order in which teams are processed for seeding.
    * **"By Creation Date"**: Sort by the timestamp of when the team was added to the system (ascending).
    * **"By Team Name"**: Sort alphabetically by the team's name (ascending).
    * **"Randomly"**: Sort in a random order.
* **Seed Placement (Radio Inputs):** Determines where the new seeds are placed relative to existing seeded teams (only relevant for **"Seed Unseeded Teams"**).
    * **"Fill Gaps"**: Attempt to use the lowest available seed number that is not currently assigned to an already seeded team.
    * **"At the End"**: Start assigning new seed numbers immediately following the **Max Existing Seed Number**.
* **Seed Gap Closure (Radio Inputs):** Post-seeding logic for ensuring final seed numbers are contiguous.
    * **"Let Gaps Remain"** (Default): Preserve any gaps or duplicates that resulted from the seeding process.
    * **"Close All Gaps"** (Treated as **Registration Closed**): After seeding, re-sequence **all** seeded teams to ensure seeds are contiguous $(1, 2, 3, \ldots, N)$ with no duplicates.

---

### 3. ⚙️ Submission Logic: Core Seeding Process

The submission (POST request) must process the selected divisions **within a database transaction** to ensure data integrity.

#### 3.1. **Method A: "Reseed All Teams"**

1.  **Selection:** Select **all** active `TournamentDivisionTeam` records for the selected division(s).
2.  **Sorting:** Sort this complete list using the selected **Sorting Method**.
3.  **Assignment:** Iterate through the sorted list and assign a new `SeedNumber` to each `TournamentDivisionTeam`, starting sequentially at **1**.
4.  **Finalize:** Apply the **Seed Gap Closure** logic (see Section 3.3).

#### 3.2. **Method B: "Seed Unseeded Teams"**

This method seeds only teams where the current `SeedNumber` is **0 or NULL**.

1.  **Selection (Unseeded):** Select all active `TournamentDivisionTeam` records where `SeedNumber` is **0 or NULL**.
2.  **Sorting (Unseeded):** Sort this unseeded list using the selected **Sorting Method**.
3.  **Selection (Seeded):** Select all active `TournamentDivisionTeam` records where `SeedNumber` is **$> 0$**.
4.  **Placement Logic:**
    * **If Seed Placement is "Fill Gaps":**
        * Start a seed counter at **1**.
        * Iterate through the **sorted unseeded teams** and assign the current counter value as the `SeedNumber`.
        * **Crucially:** Before assigning, check the **seeded teams list** if that seed number already exists. If it exists, increment the counter until an available (unused) seed number is found.
    * **If Seed Placement is "At the End":**
        * Calculate the **Max Existing Seed Number** from the **seeded teams list**. If no teams are seeded, the max is 0.
        * Start the seed counter at: **Max Existing Seed Number + 1**.
        * Iterate through the **sorted unseeded teams** and assign the sequential counter value as the `SeedNumber`.
5.  **Finalize:** Apply the **Seed Gap Closure** logic (see Section 3.3).

#### 3.3. **Seed Gap Closure Logic (Final Step for Both Methods)**

This logic is executed *only* if **"Close All Gaps"** is selected.

1.  **Consolidate:** Create a complete list of **all** `TournamentDivisionTeam` records that now have a `SeedNumber` **$> 0$** in the selected division(s).
2.  **Primary Sort:** Sort this complete list primarily by the existing `SeedNumber` (ascending).
3.  **Secondary Sort (Tiebreaker):** Apply the selected **Sorting Method** as a secondary sort for any teams with the same `SeedNumber` (duplicates).
4.  **Re-assignment:** Iterate through this final, sorted list and **re-assign** a new, sequential `SeedNumber` starting from **1** to the total count of teams.
    * **Result:** This process ensures **no gaps** in the final seed numbers (i.e., $1, 2, 3, \ldots, N$) and **no duplicate** seed numbers.
 