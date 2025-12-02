You can look at the README.md to understand the design of this solution.
Remember that both the Admin and public project make use of the Lib project.
Please run the project and fix any errors.


All CRUD views should be nicely contained in a bootstrap card.

Check the DB models and uses for correctness

in the .App project build a TeamsController which lists all teams by division, group as well as results, and also the details of an single team.
Style the views using bootstrap containers and cards.
The details view should also show an automatically updating feed of match results.

===
Create a public UpdatesController in the .App project.

1. The View (Index.cshtml):

Fetch the 25 most recent MatchUpdates from the database (Order by CreatedDate Desc).

Render a container div for the list.

Import a lightweight Markdown library (like marked.js ) via CDN.

Date Formatting: "Format the timestamps as 'TimeAgo' (e.g., '2 mins ago') using a library like moment.js or C# logic."

2. The Logic (JavaScript):

Create a function renderUpdateCard(update) that takes the update object, converts the Markdown content to HTML, and returns a styled HTML card.

On page load, iterate through the server-side model data and append these cards.

3. Real-Time (SignalR):

Connect to the existing SignalR Hub.

Listen for the 'ReceiveMatchUpdate' event.

When a new update arrives:

Pass it to renderUpdateCard.

Prepend the new card to the top of the list with a subtle fade-in animation.

Remove the last item if the count exceeds 25."

===
Update the public Matches/Details.cshtml page
1. Improve the visual presentation of the page. Also display the match number and state (scheduled, started, inprogress,finished, disputed)
2. Display the details of each MatchSet
3. Ensure the updates are rendered. Remember the content of the update is markdown and should be rendered accordingly.

====
Update the public Teams/Details.cshtml page
1. Improve the visual presentation of the page. Also display the match number and state (scheduled, started, inprogress,finished, disputed)
2. Improve the display of the match results. Include the round, match number and other relevant details.
====
On the Home index page create a card above "Total Teams" to display the details (date and description) of the active tournament.
====
The Rounds index page in the Admin controller needs alot of work. Please fix the presentation and display relevant details.
====
Fix the admin area TournamentTeams/Details.cshtml page and action.
The page needs to display all relevant informaiton about the team in the active tournament.
====
In the admin area, in the TournamentTeams controller create a new action and view.
The view will list all the teams in the system
- Beside each team we should be able to select a division (radio) and a SeedNumber. Remember an option to "remove the team" by unselecting a division
-The purpose of this interface is a fast way to bulk select teams for the tournament.
-The input should save via javascript.
-Place a link to this view on the TournamentTeams Index page next to the "Add Team" button.