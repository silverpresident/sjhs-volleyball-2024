## 🧑‍💻 Improved Prompt: Update `TournamentRounds/GenerateNextRound` Action

**Objective:** Implement the `TournamentRounds/GenerateNextRound` action, reusing the UI layout from `TournamentRounds/CreateFirstRound`. The goal is to configure the team flow and match generation for the next tournament round.

### 🎯 **Layout and Structure**

1.  **Round Selection:** Implement a drop-down selector for the round.
    * The list must include **all existing rounds**.
    * The **current round must be disabled (not selectable)**.
2.  **Two Primary Sections:** The main form body must be divided into two distinct sections:
    * **Teams Coming Into This Round** (Source Configuration)
    * **Teams Advancing to Next Round** (Destination Configuration)
3.  **Immedaite Workflow Execution**
---

### 📥 **Section 1: Teams Coming Into This Round (Source)**

This section must be **pre-populated** based on the data from the *current/previous* round. It configures the teams that will participate in the round being generated.

-  **Number of Teams** : Display the calculated number of teams coming into the new round (based on the previous round's outcome). 
-  **Method of Selection** : Display the selection method used to determine these teams (e.g., 'Winners from Previous Round', 'Top N Seeds'). 

---

### 📤 **Section 2: Teams Advancing to Next Round (Destination)**

This section configures the rules for team selection, match generation, and how these teams will move to the *subsequent* round.

- **Number of Teams** : **Input field.** Defaults to: **Half the number of teams coming into the current round.** Min 2
- **Method of Selection** : **Input field.** Defaults to: **Same as the previous round's Method of Selection.**
- **Match Generation Strategy** : **Input field.** Defaults to: **Same as the previous round's Match Generation Strategy.** |

---

### ⚙️ **Action Flags and Post-Submission Logic**

Finally, the form must include two critical flags implemented as 'now/defer' toggles: Assign Teams Flag and Generate Matches Flag. These flags govern immediate workflow execution upon form submission (The execution logic should apply to the `POST` handler for both CreateFirstRound and GenerateNextRound forms):

-  If 'Assign teams now' is selected, the system must immediately execute the SelectTeams workflow (which should be an internal service or method call) before saving the round configuration.

 - If 'Generate match now' is selected, the system must redirect the user immediately to the GenerateMatches view/action for the newly configured round ID after the round configuration has been successfully saved.
---

### 📝 **Technical Guidance**

* **Internal Service:** Ensure the **`SelectTeams` workflow** is implemented as a clean, internal service/method to maintain separation of concerns.
* **Data Structure:** The form data submitted must clearly distinguish between the **Source** and **Destination** configurations.
