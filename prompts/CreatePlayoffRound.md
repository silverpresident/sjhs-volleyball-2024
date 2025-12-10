# Feature Prompt: Create Playoff Round

This feature enables an administrator to create a "Playoff" round from a completed regular round. The new playoff round will be populated with the best-performing teams from the previous round that did not automatically advance, often referred to as the "best losers."

---

### **1. Update ViewModels and Button Logic**

To integrate the new feature cleanly into the UI, we'll update the view models and the button state logic.

*   **File:** `src/VolleyballRallyManager.Lib/Models/ITournamentRoundButtonState.cs`
*   **Action:** Add a property to the interface to control the visibility of the "Create Playoff Round" button.
    ```csharp
    bool ShowCreatePlayoffRound { get; }
    ```

*   **File:** `src/VolleyballRallyManager.Lib/Models/TournamentRoundDetailsViewModel.cs`
*   **Action:**
    1.  Ensure the `IsPlayoff` property is being mapped from the `TournamentRound` entity.
    2.  Implement the `ShowCreatePlayoffRound` property. It should return `true` only when the round is complete, rankings are available, and the round is **not** a playoff round (`IsPlayoff` is `false`).
    3.  Modify the existing `ShowGenerateNextRound` property logic to return `false` if `IsPlayoff` is `true`.

---

### **2. Service Layer Logic**

The core logic for identifying candidate teams belongs in the service layer.

*   **File:** `src/VolleyballRallyManager.Lib/Services/ITournamentRoundService.cs`
*   **Action:** Define a new method signature for retrieving playoff candidate teams.
    ```csharp
    Task<IEnumerable<TournamentRoundTeamSummaryViewModel>> GetPlayoffCandidateTeamsAsync(int previousRoundId, int numberOfTeamsToSelect);
    ```

*   **File:** `src/VolleyballRallyManager.Lib/Services/TournamentRoundService.cs`
*   **Action:** Implement the `GetPlayoffCandidateTeamsAsync` method.
    1.  Inject `IRanksService`.
    2.  Fetch all teams that participated in the `previousRoundId`.
    3.  Identify the teams that have already advanced to the next round (the "winners"). You can reuse logic from the "Generate Next Round" feature for this.
    4.  Create a list of "losers" by filtering out the winning teams.
    5.  Use `IRanksService` to rank this list of losing teams based on their performance (e.g., set win/loss ratio, then point differential).
    6.  Return the top `numberOfTeamsToSelect` teams from the ranked losers list.

---

### **3. Controller Implementation**

Create the necessary controller actions to expose the feature to the admin UI.

*   **File:** `src/VolleyballRallyManager.App/Areas/Admin/Controllers/TournamentRoundsController.cs`
*   **Action:** Add new `GET` and `POST` actions for creating a playoff round.

    *   **GET Action:**
        ```csharp
        // GET: Admin/TournamentRounds/CreatePlayoffRound/{id}
        public async Task<IActionResult> CreatePlayoffRound(int id)
        {
            // Use the GenerateNextRound GET action as a template.
            // 1. Get the previous round's details.
            // 2. Create a `CreatePlayoffRoundViewModel`.
            // 3. Use `GetPlayoffCandidateTeamsAsync` to get the list of candidate teams.
            // 4. Pre-populate the view model with recommendations (e.g., number of teams, round name).
            // 5. Return the `CreatePlayoffRound` view with the view model.
        }
        ```

    *   **POST Action:**
        ```csharp
        // POST: Admin/TournamentRounds/CreatePlayoffRound
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePlayoffRound(CreatePlayoffRoundViewModel viewModel)
        {
            // Use the GenerateNextRound POST action as a template.
            // 1. If ModelState is valid, create a new `TournamentRound` entity.
            // 2. **Crucially, set `IsPlayoff = true`.**
            // 3. Set other properties (Name, NumberOfSets, etc.) from the view model.
            // 4. Add the teams from `viewModel.SelectedTeamIds` to the new round.
            // 5. Save the new round via the service.
            // 6. Redirect to the `Details` page for the newly created playoff round.
        }
        ```

---

### **4. Create New ViewModel**

A dedicated view model is needed for the creation form.

*   **File:** `src/VolleyballRallyManager.App/Areas/Admin/Models/CreatePlayoffRoundViewModel.cs` (New File)
*   **Action:** Create a view model to carry data between the controller and the `CreatePlayoffRound` view. Use `AutoGenerateNextRoundViewModel.cs` as a reference.
    ```csharp
    public class CreatePlayoffRoundViewModel
    {
        public int PreviousRoundId { get; set; }
        public string PreviousRoundName { get; set; }

        [Required]
        [Display(Name = "Playoff Round Name")]
        public string RoundName { get; set; }

        // Include other round properties as needed (e.g., NumberOfSets, PointsToWin)

        [Display(Name = "Number of Teams to Select")]
        public int NumberOfTeamsToSelect { get; set; }

        public List<TournamentRoundTeamSummaryViewModel> CandidateTeams { get; set; } = new();
        public List<int> SelectedTeamIds { get; set; } = new();
    }
    ```

---

### **5. View Implementation**

Finally, create and update the Razor views to expose the functionality.

*   **File:** `src/VolleyballRallyManager.App/Areas/Admin/Views/TournamentRounds/Details.cshtml`
*   **Action:** Add the "Create Playoff Round" button, using the button state view model property for conditional rendering. Place it next to the "Generate Next Round" button.
    ```html
    @if (Model.ButtonState.ShowCreatePlayoffRound)
    {
        <a asp-action="CreatePlayoffRound" asp-route-id="@Model.Round.Id" class="btn btn-warning me-2">Create Playoff Round</a>
    }
    ```
    *(Note: The existing "Generate Next Round" button will be correctly hidden for playoff rounds due to the logic change in Step 1.)*

*   **File:** `src/VolleyballRallyManager.App/Areas/Admin/Views/TournamentRounds/CreatePlayoffRound.cshtml` (New File)
*   **Action:**
    1.  Create this new view, using `GenerateNextRound.cshtml` as a template to ensure visual consistency.
    2.  The view must contain a form that posts to the `CreatePlayoffRound` POST action.
    3.  Display the `CandidateTeams` in a checklist, allowing the admin to confirm or modify the final `SelectedTeamIds`.
    4.  Include form fields for the new round's properties (Name, Number of Sets, etc.).
